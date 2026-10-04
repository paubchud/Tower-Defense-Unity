using TowerDefense.Core;
using TowerDefense.Data;
using TowerDefense.Presentation;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TowerDefense.Networking
{
    [RequireComponent(typeof(NetworkObject), typeof(CharacterController), typeof(NetworkTransform))]
    public sealed class NetworkHero : NetworkBehaviour
    {
        public ContentCatalog Catalog;
        public readonly NetworkVariable<FixedString64Bytes> ClassId = new NetworkVariable<FixedString64Bytes>();
        public readonly NetworkVariable<int> Side = new NetworkVariable<int>();
        public readonly NetworkVariable<int> SelectedSlot = new NetworkVariable<int>();
        public readonly NetworkVariable<bool> Ready = new NetworkVariable<bool>();
        public readonly NetworkVariable<bool> Running = new NetworkVariable<bool>();
        public readonly NetworkVariable<FixedString64Bytes> MatchPlayerId = new NetworkVariable<FixedString64Bytes>();
        public readonly NetworkVariable<uint> Round = new NetworkVariable<uint>();
        public readonly NetworkVariable<CombatPhase> Phase = new NetworkVariable<CombatPhase>();
        public readonly NetworkVariable<int> Winner = new NetworkVariable<int>(-1);
        public readonly NetworkVariable<float> Health = new NetworkVariable<float>();
        public readonly NetworkVariable<float> MaximumHealth = new NetworkVariable<float>();
        public readonly NetworkVariable<float> CastleHealth = new NetworkVariable<float>();
        public readonly NetworkVariable<HeroLife> Life = new NetworkVariable<HeroLife>();
        public readonly NetworkVariable<double> RespawnAt = new NetworkVariable<double>();
        public readonly NetworkVariable<double> ClockOffset = new NetworkVariable<double>();
        public readonly NetworkVariable<double> FinishedAt = new NetworkVariable<double>();
        public double CombatNow => Phase.Value == CombatPhase.Finished ? FinishedAt.Value : NetworkManager.ServerTime.Time - ClockOffset.Value;
        public readonly NetworkVariable<double> HeroAttackAt = new NetworkVariable<double>(-100);
        public readonly NetworkVariable<Vector3> HeroAttackEnd = new NetworkVariable<Vector3>();
        public readonly NetworkVariable<double> TowerAttackAt = new NetworkVariable<double>(-100);
        public readonly NetworkVariable<Vector3> TowerAttackEnd = new NetworkVariable<Vector3>();
        // Private economy/energy never become an opponent's readable NetworkVariables.
        public readonly NetworkVariable<CombatEconomy> Economy = new NetworkVariable<CombatEconomy>(default, NetworkVariableReadPermission.Owner);
        public NetworkList<TroopSnapshot> Incoming;
        public NetworkList<EnergySnapshot> Energy;
        // Structure appearances are public until Milestone 4 observer filtering; reserves stay private.
        public NetworkList<PlotSnapshot> Plots;
        public NetworkList<NodeSnapshot> Nodes;
        public string CombatFeedback { get; private set; } = "Select your weapon and hold left mouse to attack.";
        public HeroClassDefinition Definition { get; private set; }
        public StarterInventory Inventory { get; private set; }
        private CharacterController controller;
        private Transform model;
        private Vector2 serverInput;
        private double lastInput;
        private float nextSend;
        private int requestedSlot;
        private uint commandSequence, observedRound;
        private float nextAttackRequest;
        private LineRenderer attackLine;
        private HeroLife displayedLife;
        public bool GrantsVision => Running.Value && Phase.Value == CombatPhase.Playing && Life.Value == HeroLife.Alive;

        private void Awake()
        {
            Incoming = new NetworkList<TroopSnapshot>();
            Energy = new NetworkList<EnergySnapshot>(null, NetworkVariableReadPermission.Owner);
            Plots = new NetworkList<PlotSnapshot>();
            Nodes = new NetworkList<NodeSnapshot>(null, NetworkVariableReadPermission.Owner);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public Vector2? SmokeInput { get; set; }
        public void SmokeTeleport(Vector3 position)
        {
            if (!IsServer) return;
            controller.enabled = false;
            GetComponent<NetworkTransform>().Teleport(Catalog.TestMap.ClampHero(position), Quaternion.identity, Vector3.one);
            controller.enabled = true;
        }
#endif

        public override void OnNetworkSpawn()
        {
            controller = GetComponent<CharacterController>();
            controller.enabled = IsServer;
            ClassId.OnValueChanged += OnClassChanged;
            SelectedSlot.OnValueChanged += OnSlotChanged;
            PrototypeSession.Instance.Heroes.Add(this);
            RefreshClass();
            displayedLife = Life.Value;
        }

        public void Initialize(string classId, int side)
        {
            if (!IsServer) return;
            ClassId.Value = new FixedString64Bytes(classId);
            Side.Value = side;
            ResetForLobby();
            Debug.Log($"TD_PLAYER class={classId} side={side} client={OwnerClientId}");
        }

        public void ResetForLobby()
        {
            if (!IsServer) return;
            Running.Value = false; Ready.Value = false; SelectedSlot.Value = 0;
            Phase.Value = CombatPhase.Lobby; Winner.Value = -1; Round.Value = 0;
            MatchPlayerId.Value = default; Life.Value = HeroLife.Alive; RespawnAt.Value = 0;
            Health.Value = Definition != null ? Definition.MaxHealth : 100;
            MaximumHealth.Value = Health.Value;
            CastleHealth.Value = Catalog.CombatRules != null ? Catalog.CombatRules.CastleHealth : 300;
            Economy.Value = default; Incoming.Clear(); Energy.Clear(); HeroAttackAt.Value = TowerAttackAt.Value = -100;
            Plots.Clear(); Nodes.Clear();
            serverInput = Vector2.zero;
            var position = Catalog.TestMap.Lanes[Side.Value].HeroSpawn;
            controller.enabled = false;
            GetComponent<NetworkTransform>().Teleport(position, Quaternion.identity, Vector3.one);
            controller.enabled = true;
        }

        private void OnClassChanged(FixedString64Bytes before, FixedString64Bytes after) => RefreshClass();
        private void OnSlotChanged(int before, int after)
        {
            if (IsOwner) requestedSlot = after;
            RefreshHeldItem();
            ApplyLifePresentation();
        }

        private void RefreshClass()
        {
            Definition = Catalog.FindClass(ClassId.Value.ToString());
            if (Definition == null) return;
            Inventory = new StarterInventory(Definition);
            requestedSlot = SelectedSlot.Value;
            if (model != null) Destroy(model.gameObject);
            model = HeroModel.Create(transform, Definition, Catalog.BaseMaterial);
            RefreshHeldItem();
            ApplyLifePresentation();
        }

        private void RefreshHeldItem()
        {
            if (model != null && Inventory != null)
                HeroModel.SetHeldItem(model, Inventory[SelectedSlot.Value], Catalog.BaseMaterial);
        }

        private void Update()
        {
            if (IsSpawned && Definition != null)
            {
                if (displayedLife != Life.Value) ApplyLifePresentation();
                DrawAttack();
            }
            if (!IsOwner || !IsSpawned || Definition == null) return;
            ObserveRound();
            var controls = PrototypeSession.Instance.Controls;
            bool pointerOnUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (!controls.BlockGameplay && !pointerOnUI)
            {
                if (controls.Harvest.WasPressedThisFrame() && Running.Value && Phase.Value == CombatPhase.Playing)
                {
                    int closest = -1; float distance = float.MaxValue;
                    foreach (var node in Nodes)
                    {
                        float candidate = Vector3.Distance(transform.position, Catalog.TestMap.ResourceNodes[node.Id]);
                        if (candidate < distance) { distance = candidate; closest = node.Id; }
                    }
                    RequestEconomy(EconomyAction.Harvest, closest);
                }
                float scroll = controls.Scroll.ReadValue<Vector2>().y;
                if (Mathf.Abs(scroll) > 0.01f) RequestSlot(Inventory.Cycle(requestedSlot, scroll > 0 ? -1 : 1));
                if (Running.Value && Phase.Value == CombatPhase.Playing && Life.Value == HeroLife.Alive && controls.Attack.IsPressed() && Time.unscaledTime >= nextAttackRequest)
                {
                    nextAttackRequest = Time.unscaledTime + Definition.AttackInterval;
                    var aimCamera = Camera.main;
                    var mouse = UnityEngine.InputSystem.Mouse.current;
                    if (aimCamera != null && mouse != null)
                    {
                        var ground = new Plane(Vector3.up, Vector3.zero);
                        var ray = aimCamera.ScreenPointToRay(mouse.position.ReadValue());
                        if (ground.Raycast(ray, out float distance))
                        {
                            Vector3 aim = ray.GetPoint(distance) - transform.position;
                            RequestAttack(new Vector2(aim.x, aim.z));
                        }
                    }
                }
            }
            if (Time.unscaledTime < nextSend) return;
            nextSend = Time.unscaledTime + 1f / 30;
            Vector2 move = controls.BlockGameplay ? Vector2.zero : controls.Move.ReadValue<Vector2>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (SmokeInput.HasValue) move = SmokeInput.Value;
#endif
            var camera = Camera.main;
            if (camera != null)
            {
                Vector3 forward = camera.transform.forward; forward.y = 0; forward.Normalize();
                Vector3 right = camera.transform.right; right.y = 0; right.Normalize();
                Vector3 world = Vector3.ClampMagnitude(forward * move.y + right * move.x, 1);
                move = new Vector2(world.x, world.z);
            }
            MoveRpc(move);
        }

        public void RequestSlot(int slot)
        {
            if (!IsOwner || Inventory == null || !Inventory.ValidSlot(slot)) return;
            requestedSlot = slot;
            SelectSlotRpc(slot);
        }

        private void ObserveRound()
        {
            if (observedRound == Round.Value) return;
            observedRound = Round.Value; commandSequence = 0; nextAttackRequest = 0;
            CombatFeedback = "Hold left mouse to attack. T sends troops; U upgrades.";
        }
        public void RequestSend() { if (IsOwner && IsSpawned) { ObserveRound(); SendTroopRpc(Round.Value, ++commandSequence); } }
        public void RequestUpgrade() { if (IsOwner && IsSpawned) { ObserveRound(); UpgradeTroopRpc(Round.Value, ++commandSequence); } }
        public void RequestAttack(Vector2 aim) { if (IsOwner && IsSpawned) { ObserveRound(); AttackRpc(Round.Value, ++commandSequence, aim); } }
        public void RequestEconomy(EconomyAction action, int target = -1, int towerIndex = 0)
        { if (IsOwner && IsSpawned) { ObserveRound(); EconomyRpc(Round.Value, ++commandSequence, action, target, towerIndex); } }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void EconomyRpc(uint round, uint sequence, EconomyAction action, int target, int towerIndex)
        {
            var match = PrototypeSession.Instance.Combat;
            if (match?.Economy == null) return;
            match.UpdateHero(Side.Value, transform.position, SelectedSlot.Value);
            match.Economy.TryAction(MatchPlayerId.Value.ToString(), round, sequence, action, target, towerIndex, out string reason);
            CombatFeedbackRpc(new FixedString128Bytes(reason));
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void SendTroopRpc(uint round, uint sequence)
        {
            var match = PrototypeSession.Instance.Combat;
            if (match == null) return;
            match.TrySend(MatchPlayerId.Value.ToString(), round, sequence, out string reason);
            CombatFeedbackRpc(new FixedString128Bytes(reason));
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void UpgradeTroopRpc(uint round, uint sequence)
        {
            var match = PrototypeSession.Instance.Combat;
            if (match == null) return;
            match.TryUpgrade(MatchPlayerId.Value.ToString(), round, sequence, out string reason);
            CombatFeedbackRpc(new FixedString128Bytes(reason));
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void AttackRpc(uint round, uint sequence, Vector2 aim)
        {
            var match = PrototypeSession.Instance.Combat;
            if (match == null) return;
            // Position/selection are sampled from host-owned state, never from the command payload.
            match.UpdateHero(Side.Value, transform.position, SelectedSlot.Value);
            match.TryAttack(MatchPlayerId.Value.ToString(), round, sequence, aim, out string reason);
            CombatFeedbackRpc(new FixedString128Bytes(reason));
        }

        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Server)]
        private void CombatFeedbackRpc(FixedString128Bytes message) => CombatFeedback = message.ToString();

        public void SyncLife(CombatMatch match)
        {
            if (!IsServer || !IsSpawned) return;
            var player = match.Players[Side.Value];
            if (Life.Value == HeroLife.Ghost && player.Life == HeroLife.Alive) TeleportHome();
            Health.Value = player.Health; MaximumHealth.Value = player.MaximumHealth; Life.Value = player.Life; RespawnAt.Value = player.RespawnAt;
        }

        public void SyncCombat(CombatMatch match)
        {
            if (!IsServer || !IsSpawned) return;
            var player = match.Players[Side.Value];
            SyncLife(match);
            MatchPlayerId.Value = new FixedString64Bytes(player.Id); Round.Value = match.Generation;
            Phase.Value = match.Phase; Winner.Value = match.Winner;
            ClockOffset.Value = System.Math.Round(NetworkManager.ServerTime.Time - match.Time, 3);
            if (match.Phase == CombatPhase.Finished) FinishedAt.Value = match.Time;
            CastleHealth.Value = match.Castles[Side.Value];
            if (Economy.Value.HarvestEnds > 0 && player.HarvestEnds == 0)
                CombatFeedbackRpc(new FixedString128Bytes(player.Stone > Economy.Value.Stone
                    ? "Mining complete. Stone added to your match inventory."
                    : "Mining canceled (life, range, tool, capacity, or match state changed)."));
            Economy.Value = new CombatEconomy { Gold = player.Gold, XP = player.XP, TroopLevel = player.TroopLevel,
                Stone = player.Stone, HeroLevel = player.HeroLevel, HarvestNode = player.HarvestNode, HarvestEnds = player.HarvestEnds };
            if (match.Economy != null)
            {
                foreach (var plot in match.Economy.Plots)
                {
                    if (plot.Side != Side.Value) continue;
                    var state = new PlotSnapshot { Id = plot.Id, Owned = plot.Owned, TowerIndex = plot.TowerIndex,
                        AttackAt = plot.LastAttack, AttackEnd = plot.AttackEnd };
                    int found = -1;
                    for (int i = 0; i < Plots.Count; i++) if (Plots[i].Id == plot.Id) { found = i; break; }
                    if (found < 0) Plots.Add(state); else if (!Plots[found].Equals(state)) Plots[found] = state;
                }
                foreach (var node in match.Economy.Nodes)
                {
                    if (node.Side != Side.Value) continue;
                    var state = new NodeSnapshot { Id = node.Id, Remaining = node.Remaining, RecoverAt = node.RecoverAt };
                    int found = -1;
                    for (int i = 0; i < Nodes.Count; i++) if (Nodes[i].Id == node.Id) { found = i; break; }
                    if (found < 0) Nodes.Add(state); else if (!Nodes[found].Equals(state)) Nodes[found] = state;
                }
            }
            HeroAttackAt.Value = player.LastHeroAttack; HeroAttackEnd.Value = player.HeroAttackEnd;
            TowerAttackAt.Value = player.LastTowerAttack; TowerAttackEnd.Value = player.TowerAttackEnd;
            foreach (var pool in player.Energies.Values)
            {
                var state = new EnergySnapshot { Id = new FixedString64Bytes(pool.Id), Current = Mathf.Floor(pool.Current * 10) / 10, Capacity = pool.Capacity };
                int found = -1;
                for (int i = 0; i < Energy.Count; i++) if (Energy[i].Id == state.Id) { found = i; break; }
                if (found < 0) Energy.Add(state); else if (!Energy[found].Equals(state)) Energy[found] = state;
            }
            for (int i = Incoming.Count - 1; i >= 0; i--)
            {
                bool exists = false;
                foreach (var troop in match.Troops) if (troop.Lane == Side.Value && troop.Id == Incoming[i].Id) { exists = true; break; }
                if (!exists) Incoming.RemoveAt(i);
            }
            foreach (var troop in match.Troops)
            {
                if (troop.Lane != Side.Value) continue;
                var state = new TroopSnapshot { Id = troop.Id, Level = troop.Level, Health = troop.Health, SpawnAt = troop.SpawnAt };
                int found = -1;
                for (int i = 0; i < Incoming.Count; i++) if (Incoming[i].Id == troop.Id) { found = i; break; }
                if (found < 0) Incoming.Add(state); else if (!Incoming[found].Equals(state)) Incoming[found] = state;
            }
        }

        private void TeleportHome()
        {
            serverInput = Vector2.zero;
            controller.enabled = false;
            GetComponent<NetworkTransform>().Teleport(Catalog.TestMap.Lanes[Side.Value].HeroSpawn, Quaternion.identity, Vector3.one);
            controller.enabled = true;
        }

        private void ApplyLifePresentation()
        {
            displayedLife = Life.Value;
            if (model == null) return;
            foreach (var renderer in model.GetComponentsInChildren<Renderer>())
            {
                var block = new MaterialPropertyBlock();
                if (Life.Value == HeroLife.Ghost) block.SetColor("_BaseColor", new Color(0.5f, 0.85f, 1));
                renderer.SetPropertyBlock(block);
            }
        }

        private void DrawAttack()
        {
            bool visible = Running.Value && Phase.Value == CombatPhase.Playing && CombatNow - HeroAttackAt.Value < 0.15;
            if (!visible) { if (attackLine != null) attackLine.enabled = false; return; }
            if (attackLine == null)
            {
                var line = new GameObject("Attack trace"); line.transform.SetParent(transform, false);
                attackLine = line.AddComponent<LineRenderer>(); attackLine.positionCount = 2; attackLine.useWorldSpace = true;
                attackLine.startWidth = 0.12f; attackLine.endWidth = 0.06f;
                attackLine.sharedMaterial = WorldGeometry.MaterialFor(Definition.AccentColor, Catalog.BaseMaterial);
            }
            attackLine.enabled = true;
            attackLine.SetPosition(0, transform.position + Vector3.up);
            attackLine.SetPosition(1, HeroAttackEnd.Value + Vector3.up);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner, Delivery = RpcDelivery.Unreliable)]
        private void MoveRpc(Vector2 input)
        {
            if (!float.IsFinite(input.x) || !float.IsFinite(input.y)) return;
            serverInput = Vector2.ClampMagnitude(input, 1);
            lastInput = NetworkManager.ServerTime.Time;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        private void SelectSlotRpc(int slot)
        {
            if (Inventory != null && Inventory.ValidSlot(slot)) SelectedSlot.Value = slot;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        public void SetReadyRpc(bool ready)
        {
            if (Running.Value) return;
            Ready.Value = ready;
            PrototypeSession.Instance.TryBeginMatch();
        }

        private void FixedUpdate()
        {
            if (!IsSpawned || !IsServer || Definition == null || !Running.Value || Phase.Value != CombatPhase.Playing) return;
            Vector2 move = NetworkManager.ServerTime.Time - lastInput < 0.25 ? serverInput : Vector2.zero;
            Vector3 velocity = new Vector3(move.x, 0, move.y) * Definition.MovementSpeed;
            controller.Move((velocity + Vector3.down * 3) * Time.fixedDeltaTime);
            Vector3 clamped = Catalog.TestMap.ClampHero(transform.position);
            if (Life.Value == HeroLife.Ghost)
                clamped.x = Side.Value == 0 ? Mathf.Min(clamped.x, -1) : Mathf.Max(clamped.x, 1);
            if (clamped != transform.position) transform.position = clamped;
            if (velocity.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(velocity);
        }

        public override void OnNetworkDespawn()
        {
            ClassId.OnValueChanged -= OnClassChanged;
            SelectedSlot.OnValueChanged -= OnSlotChanged;
            if (PrototypeSession.Instance != null) PrototypeSession.Instance.Heroes.Remove(this);
        }
    }
}
