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
        public HeroClassDefinition Definition { get; private set; }
        public StarterInventory Inventory { get; private set; }
        private CharacterController controller;
        private Transform model;
        private Vector2 serverInput;
        private double lastInput;
        private float nextSend;
        private int requestedSlot;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public Vector2? SmokeInput { get; set; }
#endif

        public override void OnNetworkSpawn()
        {
            controller = GetComponent<CharacterController>();
            controller.enabled = IsServer;
            ClassId.OnValueChanged += OnClassChanged;
            SelectedSlot.OnValueChanged += OnSlotChanged;
            PrototypeSession.Instance.Heroes.Add(this);
            RefreshClass();
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
        }

        private void RefreshHeldItem()
        {
            if (model != null && Inventory != null)
                HeroModel.SetHeldItem(model, Inventory[SelectedSlot.Value], Catalog.BaseMaterial);
        }

        private void Update()
        {
            if (!IsOwner || !IsSpawned || Definition == null) return;
            var controls = PrototypeSession.Instance.Controls;
            bool pointerOnUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (!controls.BlockGameplay && !pointerOnUI)
            {
                float scroll = controls.Scroll.ReadValue<Vector2>().y;
                if (Mathf.Abs(scroll) > 0.01f) RequestSlot(Inventory.Cycle(requestedSlot, scroll > 0 ? -1 : 1));
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
            if (!IsSpawned || !IsServer || Definition == null || !Running.Value) return;
            Vector2 move = NetworkManager.ServerTime.Time - lastInput < 0.25 ? serverInput : Vector2.zero;
            Vector3 velocity = new Vector3(move.x, 0, move.y) * Definition.MovementSpeed;
            controller.Move((velocity + Vector3.down * 3) * Time.fixedDeltaTime);
            Vector3 clamped = Catalog.TestMap.ClampHero(transform.position);
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
