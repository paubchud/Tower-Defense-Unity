using System.Collections.Generic;
using System.Text;
using TowerDefense.Core;
using TowerDefense.Data;
using TowerDefense.Networking;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    public sealed class PrototypeView : MonoBehaviour
    {
        public bool MatchView;
        private PrototypeSession session;
        private RectTransform canvasRoot;
        private RectTransform page;
        private RectTransform modal;
        private Text status;
        private Text heroLabel;
        private Text roster;
        private Text readyLabel;
        private Button ready;
        private RectTransform lobby;
        private Text roomLabel;
        private Button copyCode;
        private Button inviteFriend;
        private InputField roomCodeField;
        private string observedInvite;
        private bool connectionPage;
        private bool signInPage;
        private StartupSignInState observedSignIn;
        private string observedStatus;
        private readonly List<Selectable> connectionWidgets = new List<Selectable>();
        private readonly Text[] slotLabels = new Text[3];
        private readonly Image[] slotImages = new Image[3];
        private float nextRefresh;
        private string selectedClass;
        private Font font;
        private static readonly Color Ink = new Color(0.055f, 0.075f, 0.11f);
        private static readonly Color Panel = new Color(0.1f, 0.13f, 0.18f, 0.97f);
        private static readonly Color Gold = new Color(0.87f, 0.73f, 0.42f);
        private static readonly Color Muted = new Color(0.63f, 0.7f, 0.77f);

        private void Start()
        {
            session = PrototypeSession.Instance;
            if (session == null) { Debug.LogError("Prototype bootstrap is missing. Open MainMenu first."); return; }
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = new GameObject("Prototype UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.transform.SetParent(transform, false);
            canvasRoot = (RectTransform)canvas.transform;
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440, 900);
            scaler.matchWidthOrHeight = 0.5f;
            if (EventSystem.current == null)
            {
                var eventObject = new GameObject("Event System", typeof(EventSystem), typeof(InputSystemUIInputModule));
                eventObject.transform.SetParent(transform, false);
                eventObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            session.Controls.PanelRequested += OnPanelRequested;
            if (MatchView) BuildArenaHud();
            else if (session.SignIn.Ready) BuildMainMenu();
            else BuildSignIn();
        }

        private RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size, Vector2 anchor)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = size; rect.anchoredPosition = position;
            return rect;
        }

        private RectTransform Box(string name, Transform parent, Vector2 position, Vector2 size, Vector2 anchor, Color color)
        {
            var rect = Rect(name, parent, position, size, anchor);
            rect.gameObject.AddComponent<Image>().color = color;
            return rect;
        }

        private Text Label(Transform parent, string text, Vector2 position, Vector2 size, int fontSize,
            Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var rect = Rect("Label", parent, position, size, new Vector2(0.5f, 0.5f));
            var label = rect.gameObject.AddComponent<Text>();
            label.font = font; label.text = text; label.fontSize = fontSize;
            label.color = color; label.alignment = alignment; label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        private Button ActionButton(Transform parent, string text, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action, bool primary = false)
        {
            var rect = Box(text, parent, position, size, new Vector2(0.5f, 0.5f), primary ? Gold : new Color(0.16f, 0.21f, 0.28f));
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            button.onClick.AddListener(action);
            Label(rect, text, Vector2.zero, size - new Vector2(12, 4), 20, primary ? Ink : Color.white);
            return button;
        }

        private void NewPage()
        {
            if (page != null) { page.gameObject.SetActive(false); Destroy(page.gameObject); }
            connectionPage = false;
            signInPage = false;
            roomCodeField = null;
            connectionWidgets.Clear();
            page = Box("Menu page", canvasRoot, Vector2.zero, Vector2.zero, Vector2.one * 0.5f, Ink);
            page.anchorMin = Vector2.zero; page.anchorMax = Vector2.one; page.sizeDelta = Vector2.zero;
            Label(page, "TOWER DEFENSE", new Vector2(0, 270), new Vector2(900, 105), 64, Color.white);
            Label(page, "TWO CASTLES. ONE VICTOR.", new Vector2(0, 195), new Vector2(900, 45), 21, Gold);
            Label(page, (session.OnlineProvider is SteamOnlineProvider && session.SteamService.PrivatePlaytest ? "PRIVATE STEAM TEST / Spacewar 480 / v" : "Private 1v1 prototype / v") + Application.version,
                new Vector2(0, -370), new Vector2(900, 30), 16, Muted);
        }

        private void BuildMainMenu()
        {
            if (!session.SignIn.Ready) { BuildSignIn(); return; }
            NewPage();
            var profile = Label(page, session.SignIn.Account.Provider == "guest" ? "Signed in: Guest (local profile)" :
                "Signed in: " + session.SignIn.Flow.DisplayName + (session.SignIn.Account.Provider == "steam-test" ? " / Steam development test" : " / Steam"),
                new Vector2(0, 140), new Vector2(1000, 35), 18, Muted);
            profile.supportRichText = false;
            ActionButton(page, "PLAY", new Vector2(0, 60), new Vector2(310, 65), BuildClassSelection, true);
            ActionButton(page, "STORE", new Vector2(0, -30), new Vector2(310, 65), BuildStore);
            ActionButton(page, "EXIT", new Vector2(0, -120), new Vector2(310, 65), Exit);
        }

        private void BuildSignIn()
        {
            NewPage();
            signInPage = true;
            observedSignIn = session.SignIn.Flow.State;
            if (observedSignIn == StartupSignInState.CheckingSteam)
            {
                Label(page, "CHECKING STEAM...", new Vector2(0, 35), new Vector2(900, 70), 28, Color.white);
                ActionButton(page, "EXIT", new Vector2(0, -225), new Vector2(280, 48), Exit);
                return;
            }
            Label(page, "CONTINUE AS A GUEST", new Vector2(0, 105), new Vector2(900, 50), 30, Color.white);
            Label(page, "No account or password required. Your guest profile stays on this device.\nGuest internet matchmaking is not connected yet; LAN testing is available.",
                new Vector2(0, 35), new Vector2(900, 65), 18, Muted);
            ActionButton(page, "PLAY AS GUEST", new Vector2(-155, -45), new Vector2(285, 58), () =>
            {
                if (session.ContinueAsGuest()) BuildMainMenu();
                else status.text = session.SignIn.Flow.Status;
            }, true);
            ActionButton(page, "RETRY STEAM", new Vector2(155, -45), new Vector2(285, 58), () =>
            { session.SignIn.RetrySteam(); BuildSignIn(); });
            status = Label(page, session.SignIn.Flow.Status, new Vector2(0, -150), new Vector2(950, 95), 17, Muted);
            ActionButton(page, "EXIT", new Vector2(0, -245), new Vector2(280, 48), Exit);
            if (session.SteamService.PrivatePlaytestAvailable && !session.SteamService.PrivatePlaytest)
                ActionButton(page, "ENABLE PRIVATE STEAM TEST (480)", new Vector2(0, -315), new Vector2(520, 40), () =>
                {
                    if (session.EnablePrivateSteamPlaytest()) { session.SignIn.RetrySteam(); BuildSignIn(); }
                    else status.text = session.Status;
                });
        }

        private void BuildStore()
        {
            NewPage();
            Label(page, "Choose your playstyle", new Vector2(0, 85), new Vector2(650, 50), 30, Color.white);
            Label(page, "Earn currency by playing to unlock classes, starter alternatives,\nand customization. The store has no stock in this first test.",
                new Vector2(0, -15), new Vector2(780, 120), 21, Muted);
            ActionButton(page, "BACK", new Vector2(0, -155), new Vector2(280, 55), BuildMainMenu);
        }

        private void BuildClassSelection()
        {
            NewPage();
            int count = session.Catalog.Classes.Length;
            for (int i = 0; i < count; i++)
            {
                var definition = session.Catalog.Classes[i];
                float x = (i - (count - 1) * 0.5f) * 350;
                var card = Box(definition.DisplayName, page, new Vector2(x, 0), new Vector2(320, 270), Vector2.one * 0.5f, Panel);
                Label(card, definition.DisplayName.ToUpperInvariant(), new Vector2(0, 85), new Vector2(290, 45), 32, definition.AccentColor);
                Label(card, definition.TechnologyGroup.DisplayName + "  /  " + definition.Difficulty,
                    new Vector2(0, 35), new Vector2(290, 35), 17, Gold);
                Label(card, definition.Description, new Vector2(0, -25), new Vector2(280, 90), 18, Muted);
                ActionButton(card, "SELECT", new Vector2(0, -95), new Vector2(260, 45), () => BuildConnection(definition.Id), true);
            }
            ActionButton(page, "BACK", new Vector2(0, -235), new Vector2(240, 50), BuildMainMenu);
        }

        private InputField Field(Transform parent, string value, Vector2 position, Vector2 size)
        {
            var rect = Box("Input", parent, position, size, Vector2.one * 0.5f, Panel);
            var input = rect.gameObject.AddComponent<InputField>();
            input.textComponent = Label(rect, value, Vector2.zero, size - new Vector2(24, 4), 22, Color.white, TextAnchor.MiddleLeft);
            input.text = value; input.targetGraphic = rect.GetComponent<Image>();
            return input;
        }

        private void BuildConnection(string classId)
        {
            selectedClass = classId;
            NewPage();
            connectionPage = true;
            var definition = session.Catalog.FindClass(classId);
            Label(page, definition.DisplayName + " / " + definition.TechnologyGroup.DisplayName,
                new Vector2(0, 100), new Vector2(780, 45), 28, definition.AccentColor);
            Label(page, "ROOM CODE", new Vector2(0, 50), new Vector2(700, 35), 17, Muted);
            var provider = session.OnlineProvider;
            var code = Field(page, provider.PendingInvite, new Vector2(0, 5), new Vector2(480, 48));
            roomCodeField = code;
            observedInvite = provider.PendingInvite;
            code.characterLimit = 128; // Each provider validates its own code format before any service call.
            var placeholder = Label(code.transform, "Code from your friend", Vector2.zero, new Vector2(336, 44), 20, Muted, TextAnchor.MiddleLeft);
            code.placeholder = placeholder;
            Label(page, provider.DisplayName + " connection. Host or paste the room code from your friend.", new Vector2(0, -40), new Vector2(900, 35), 17, Muted);
            connectionWidgets.Add(code);
            bool guestUnavailable = provider is UnconfiguredGuestProvider;
            var hostButton = ActionButton(page, "HOST " + provider.DisplayName.ToUpperInvariant(), new Vector2(-155, -100), new Vector2(270, 58), () =>
            {
                _ = session.ConnectOnlineAsync(true, selectedClass);
                status.text = session.Status;
            }, true);
            var joinButton = ActionButton(page, "JOIN " + provider.DisplayName.ToUpperInvariant(), new Vector2(155, -100), new Vector2(270, 58), () =>
            {
                _ = session.ConnectOnlineAsync(false, selectedClass, code.text);
                status.text = session.Status;
            });
            hostButton.interactable = joinButton.interactable = !guestUnavailable;
            if (!guestUnavailable) { connectionWidgets.Add(hostButton); connectionWidgets.Add(joinButton); }
            status = Label(page, provider is SteamOnlineProvider && session.SteamService.PrivatePlaytest
                ? "PRIVATE DEVELOPMENT TEST / App ID 480. Both players enable this mode, use the same build and separate Steam accounts/devices. Share a numeric code with your friend."
                : guestUnavailable ? UnconfiguredGuestProvider.SetupMessage : provider.DisplayName + " account + room networking. Use the same build. Steam and guest rooms are separate.",
                new Vector2(0, -185), new Vector2(850, 90), 17, Muted);
            observedStatus = session.Status;
            connectionWidgets.Add(ActionButton(page, "LAN / THIS PC", new Vector2(150, -275), new Vector2(270, 48), () => BuildLanConnection(classId)));
            ActionButton(page, "BACK / CANCEL", new Vector2(-150, -275), new Vector2(270, 48), BackFromConnection);
        }

        private void BackFromConnection()
        {
            if (session.Connecting) session.Leave();
            else BuildClassSelection();
        }

        private void BuildLanConnection(string classId)
        {
            selectedClass = classId;
            NewPage();
            connectionPage = true;
            var definition = session.Catalog.FindClass(classId);
            Label(page, definition.DisplayName + " / " + definition.TechnologyGroup.DisplayName,
                new Vector2(0, 100), new Vector2(780, 45), 28, definition.AccentColor);
            Label(page, "Host address", new Vector2(-105, 45), new Vector2(230, 35), 17, Muted, TextAnchor.MiddleLeft);
            Label(page, "Port", new Vector2(205, 45), new Vector2(100, 35), 17, Muted, TextAnchor.MiddleLeft);
            var address = Field(page, "127.0.0.1", new Vector2(-80, 0), new Vector2(290, 48));
            var port = Field(page, "7777", new Vector2(190, 0), new Vector2(170, 48));
            connectionWidgets.Add(address); connectionWidgets.Add(port);
            void Connect(bool host)
            {
                if (!ushort.TryParse(port.text, out ushort parsed) || parsed == 0)
                { status.text = "Enter a port between 1 and 65535."; return; }
                session.Connect(host, selectedClass, address.text, parsed);
                status.text = session.Status;
            }
            connectionWidgets.Add(ActionButton(page, "HOST LAN", new Vector2(-155, -90), new Vector2(270, 58), () => Connect(true), true));
            connectionWidgets.Add(ActionButton(page, "JOIN LAN", new Vector2(155, -90), new Vector2(270, 58), () => Connect(false)));
            status = Label(page, "For two copies on this PC, use 127.0.0.1. On a LAN, enter the host PC's address.",
                new Vector2(0, -175), new Vector2(830, 70), 17, Muted);
            observedStatus = session.Status;
            ActionButton(page, "BACK / CANCEL", new Vector2(-150, -265), new Vector2(270, 48), BackFromConnection);
            connectionWidgets.Add(ActionButton(page, "ONLINE", new Vector2(150, -265), new Vector2(270, 48), () => BuildConnection(classId)));
        }

        private void BuildArenaHud()
        {
            var header = Box("Header", canvasRoot, new Vector2(0, -43), new Vector2(1360, 70), new Vector2(0.5f, 1), Panel);
            heroLabel = Label(header, "Entering arena...", new Vector2(-255, 0), new Vector2(770, 54), 22, Color.white, TextAnchor.MiddleLeft);
            ActionButton(header, "MENU", new Vector2(550, 0), new Vector2(145, 42), session.Leave);
            ActionButton(header, "PAUSE", new Vector2(380, 0), new Vector2(145, 42), () => OpenPanel("Pause"));
            var footer = Rect("Hotbar", canvasRoot, new Vector2(0, 68), new Vector2(630, 64), new Vector2(0.5f, 0));
            for (int i = 0; i < 3; i++)
            {
                int slot = i;
                var button = ActionButton(footer, "Empty", new Vector2((i - 1) * 210, 0), new Vector2(200, 62), () => session.LocalHero?.RequestSlot(slot));
                slotImages[i] = button.GetComponent<Image>();
                slotLabels[i] = button.GetComponentInChildren<Text>(); slotLabels[i].fontSize = 17;
            }
            var help = Rect("Controls", canvasRoot, new Vector2(0, 22), new Vector2(1360, 30), new Vector2(0.5f, 0));
            Label(help, "WASD move  |  Right-drag orbit  |  Wheel hotbar  |  B shop  U upgrades  T troops  I equipment  |  Esc pause",
                Vector2.zero, new Vector2(1360, 30), 16, Color.white);
            lobby = Box("Ready lobby", canvasRoot, Vector2.zero, new Vector2(590, 375), Vector2.one * 0.5f, Panel);
            Label(lobby, session.OnlineProvider is SteamOnlineProvider && session.SteamService.PrivatePlaytest ? "PRIVATE STEAM TEST / 480" : "YOUR ARENA",
                new Vector2(0, 145), new Vector2(540, 45), 30, Gold);
            roomLabel = Label(lobby, string.Empty, new Vector2(0, 100), new Vector2(540, 40), 24, Gold);
            roster = Label(lobby, "Waiting for players...", new Vector2(0, 30), new Vector2(530, 90), 20, Color.white);
            ready = ActionButton(lobby, "READY", new Vector2(0, -68), new Vector2(280, 48), () =>
            {
                var hero = session.LocalHero;
                if (hero != null) hero.SetReadyRpc(!hero.Ready.Value);
            }, true);
            readyLabel = ready.GetComponentInChildren<Text>();
            copyCode = ActionButton(lobby, "COPY ROOM CODE", new Vector2(-135, -120), new Vector2(255, 38), CopyRoomCode);
            inviteFriend = ActionButton(lobby, "INVITE FRIEND", new Vector2(135, -120), new Vector2(255, 38), session.InviteFriend);
            status = Label(lobby, string.Empty, new Vector2(0, -165), new Vector2(550, 40), 15, Muted);
        }

        private void CopyRoomCode()
        {
            if (!string.IsNullOrEmpty(session.JoinCode)) GUIUtility.systemCopyBuffer = session.JoinCode;
        }

        private void OnPanelRequested(string name)
        {
            if (!MatchView) return;
            if (modal != null) { ClosePanel(); return; }
            OpenPanel(name);
        }

        public void OpenPanel(string name)
        {
            if (modal != null) ClosePanel();
            modal = Box("Management panel", canvasRoot, Vector2.zero, new Vector2(620, 400), Vector2.one * 0.5f, Panel);
            Label(modal, name.ToUpperInvariant(), new Vector2(0, 145), new Vector2(550, 55), 32, Gold);
            string text;
            var hero = session.LocalHero;
            if (name == "Equipment" && hero != null && hero.Definition != null)
            {
                var description = new StringBuilder();
                description.AppendLine("Weapon: " + (hero.Inventory[0]?.DisplayName ?? "None"));
                description.AppendLine("Clothing: " + (hero.Inventory.Armor?.DisplayName ?? "Unarmored"));
                description.AppendLine("Tool: " + (hero.Inventory[1]?.DisplayName ?? "None"));
                description.AppendLine("\nTower set: " + hero.Definition.TechnologyGroup.DisplayName);
                foreach (var energy in hero.Definition.Energies)
                    description.AppendLine(energy.Definition.DisplayName + " capacity: " + energy.Capacity);
                text = description.ToString();
            }
            else if (name == "Pause")
                text = (session.Online ? "Room code: " + session.JoinCode + "\n\n" : string.Empty) + "The multiplayer arena keeps running while this menu is open.\n\nWASD: move\nRight-click drag: orbit around your hero\nMouse wheel: select a hotbar item";
            else if (name == "Shop") text = "No stock available yet.\n\nYour match shop will trade items and materials for your side.";
            else if (name == "Upgrades") text = "No upgrades available yet.\n\nUnit upgrades spend match XP. Hero growth uses match gold.";
            else text = "No troops available yet.\n\nSent units will attack the opposing castle along its lane.";
            Label(modal, text, new Vector2(0, 15), new Vector2(550, 220), 20, Color.white);
            if (name == "Pause" && session.Manager.IsServer)
                ActionButton(modal, "RESET TO LOBBY", new Vector2(-140, -145), new Vector2(260, 48), () => { session.ResetLobby(); ClosePanel(); });
            ActionButton(modal, "CLOSE", new Vector2(name == "Pause" && session.Manager.IsServer ? 140 : 0, -145), new Vector2(260, 48), ClosePanel, true);
            session.Controls.BlockGameplay = true;
        }

        public void ClosePanel()
        {
            if (modal != null) Destroy(modal.gameObject);
            modal = null;
            session.Controls.BlockGameplay = false;
        }

        private void Update()
        {
            if (session == null || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.15f;
            if (!MatchView)
            {
                if (signInPage)
                {
                    if (session.SignIn.Ready) BuildMainMenu();
                    else if (observedSignIn != session.SignIn.Flow.State) BuildSignIn();
                    return;
                }
                if (!connectionPage) return;
                foreach (var widget in connectionWidgets) widget.interactable = session.CanConnect;
                string incoming = session.OnlineProvider.PendingInvite;
                if (roomCodeField != null && session.CanConnect && !string.IsNullOrEmpty(incoming) && incoming != observedInvite)
                { roomCodeField.text = incoming; observedInvite = incoming; } // Accepted invite, never an automatic class/match switch.
                if (session.Status != observedStatus) { observedStatus = session.Status; status.text = session.Status; }
                return;
            }
            var hero = session.LocalHero;
            bool running = hero != null && hero.Running.Value;
            lobby.gameObject.SetActive(!running);
            ready.interactable = hero != null && hero.Definition != null && session.Manager.IsListening;
            readyLabel.text = hero != null && hero.Ready.Value ? "UNREADY" : "READY";
            status.text = session.Status;
            roomLabel.text = session.Online ? (string.IsNullOrEmpty(session.JoinCode) ? "Connecting online..." : "ROOM CODE: " + session.JoinCode) : "LOCAL NETWORK MATCH";
            copyCode.gameObject.SetActive(session.Online && session.Manager.IsHost && !string.IsNullOrEmpty(session.JoinCode));
            inviteFriend.gameObject.SetActive(session.Online && session.Manager.IsHost && session.OnlineProvider.SupportsFriendInvites);
            session.Controls.BlockGameplay = modal != null || !running;
            var players = new StringBuilder();
            foreach (var player in session.Heroes)
                if (player.Definition != null)
                    players.AppendLine($"Side {player.Side.Value + 1}: {player.Definition.DisplayName} / {player.Definition.TechnologyGroup.DisplayName} — {(player.Ready.Value ? "Ready" : "Not ready")}");
            if (session.Heroes.Count < 2) players.AppendLine("Waiting for opponent...");
            roster.text = players.ToString();
            if (hero == null || hero.Definition == null) return;
            heroLabel.text = $"{hero.Definition.DisplayName.ToUpperInvariant()}  /  {hero.Definition.TechnologyGroup.DisplayName}  /  SIDE {hero.Side.Value + 1}";
            for (int i = 0; i < slotLabels.Length; i++)
            {
                slotLabels[i].text = hero.Inventory[i]?.DisplayName ?? "Empty";
                slotImages[i].color = i == hero.SelectedSlot.Value ? Gold : Panel;
                slotLabels[i].color = i == hero.SelectedSlot.Value ? Ink : Color.white;
            }
        }

        private void OnDestroy()
        {
            if (session != null && session.Controls != null) session.Controls.PanelRequested -= OnPanelRequested;
        }

        private static void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
