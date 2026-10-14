using System;
using System.Reflection;
using MelonLoader;
using UnityEngine;
using ArfTattleHook.UI;

namespace ArfTattleHook.Modules
{
    public class BhopModule : IModModule
    {
        public static BhopModule Instance { get; private set; }

        public string Id => "BunnyHop";
        public string Name => "BunnyHop";
        public string Description => "Auto-Jump, control de gravedad, inercia de air strafe & velocidad uncapped";
        public string Category => "Movement";

        public bool Enabled
        {
            get => _prefEnabled?.Value ?? true;
            set
            {
                if (_prefEnabled != null)
                {
                    _prefEnabled.Value = value;
                    _prefCategory.SaveToFile();
                }
                if (!value)
                {
                    CurrentSpeed = 0f;
                    IsBhopping = false;
                    RestoreVanillaPhysics();
                }
            }
        }

        public bool IsExpanded { get; set; } = true;

        public bool AutoJump
        {
            get => _prefAutoJump?.Value ?? true;
            set { if (_prefAutoJump != null) { _prefAutoJump.Value = value; _prefCategory.SaveToFile(); } }
        }

        public float JumpSpeed
        {
            get => _prefJumpSpeed?.Value ?? 4.5f;
            set { if (_prefJumpSpeed != null) { _prefJumpSpeed.Value = value; _prefCategory.SaveToFile(); } }
        }

        public float GravityMultiplier
        {
            get => _prefGravityMultiplier?.Value ?? 2.4f;
            set { if (_prefGravityMultiplier != null) { _prefGravityMultiplier.Value = value; _prefCategory.SaveToFile(); } }
        }

        public float Acceleration
        {
            get => _prefAcceleration?.Value ?? 2.5f;
            set { if (_prefAcceleration != null) { _prefAcceleration.Value = value; _prefCategory.SaveToFile(); } }
        }

        public float BoostMultiplier
        {
            get => _prefBoostMultiplier?.Value ?? 3.0f;
            set { if (_prefBoostMultiplier != null) { _prefBoostMultiplier.Value = value; _prefCategory.SaveToFile(); } }
        }

        public float AirFriction
        {
            get => _prefAirFriction?.Value ?? 0.5f;
            set { if (_prefAirFriction != null) { _prefAirFriction.Value = value; _prefCategory.SaveToFile(); } }
        }

        public bool CanLoseSpeed
        {
            get => _prefCanLoseSpeed?.Value ?? true;
            set { if (_prefCanLoseSpeed != null) { _prefCanLoseSpeed.Value = value; _prefCategory.SaveToFile(); } }
        }

        public bool ShowSpeedometer
        {
            get => _prefShowSpeedometer?.Value ?? true;
            set { if (_prefShowSpeedometer != null) { _prefShowSpeedometer.Value = value; _prefCategory.SaveToFile(); } }
        }

        public float CurrentSpeed { get; private set; } = 0f;
        public bool IsBhopping { get; private set; } = false;
        public bool IsBhopActive => Enabled && IsBhopping && CurrentSpeed > 0.1f;

        private MelonPreferences_Category _prefCategory;
        private MelonPreferences_Entry<bool> _prefEnabled;
        private MelonPreferences_Entry<bool> _prefAutoJump;
        private MelonPreferences_Entry<float> _prefJumpSpeed;
        private MelonPreferences_Entry<float> _prefGravityMultiplier;
        private MelonPreferences_Entry<float> _prefAcceleration;
        private MelonPreferences_Entry<float> _prefAirFriction;
        private MelonPreferences_Entry<float> _prefGroundFriction;
        private MelonPreferences_Entry<float> _prefBoostMultiplier;
        private MelonPreferences_Entry<bool> _prefCanLoseSpeed;
        private MelonPreferences_Entry<bool> _prefShowSpeedometer;

        private float _jumpDebounce = 0f;
        private static FieldInfo _fldJump = null;
        private static FieldInfo _fldJumpSpeed = null;
        private static FieldInfo _fldGravityMultiplier = null;
        private static FieldInfo _fldCharController = null;
        private static FieldInfo _fldInput = null;

        private float _vanillaJumpSpeed = -1f;
        private float _vanillaGravityMultiplier = -1f;

        public BhopModule()
        {
            Instance = this;
        }

        public void OnInitialize()
        {
            try
            {
                _prefCategory = MelonPreferences.CreateCategory("ArfTattleHook_Bhop", "BunnyHop Settings");
                _prefEnabled = _prefCategory.CreateEntry("Enabled", true, "Enable BunnyHop");
                _prefAutoJump = _prefCategory.CreateEntry("AutoJump", true, "Auto-jump while holding Space");
                _prefJumpSpeed = _prefCategory.CreateEntry("JumpSpeed", 4.5f, "Jump velocity speed (prevents hitting ceiling)");
                _prefGravityMultiplier = _prefCategory.CreateEntry("GravityMultiplier", 2.4f, "Gravity multiplier for snappy falling");
                _prefAcceleration = _prefCategory.CreateEntry("Acceleration", 2.5f, "Air strafe acceleration");
                _prefAirFriction = _prefCategory.CreateEntry("AirFriction", 0.5f, "Air friction deceleration");
                _prefGroundFriction = _prefCategory.CreateEntry("GroundFriction", 12.0f, "Ground friction deceleration");
                _prefBoostMultiplier = _prefCategory.CreateEntry("BoostMultiplier", 3.0f, "Shift boost multiplier");
                _prefCanLoseSpeed = _prefCategory.CreateEntry("CanLoseSpeed", true, "Lose speed in air without strafe");
                _prefShowSpeedometer = _prefCategory.CreateEntry("ShowSpeedometer", true, "Show Speedometer HUD");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[Bhop] Could not load preferences: {ex.Message}");
            }

            InitFields();
        }

        private static void InitFields()
        {
            if (_fldJump != null) return;
            try
            {
                Type t = typeof(FirstPersonController);
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
                _fldJump = t.GetField("m_Jump", flags);
                _fldJumpSpeed = t.GetField("m_JumpSpeed", flags);
                _fldGravityMultiplier = t.GetField("m_GravityMultiplier", flags);
                _fldCharController = t.GetField("m_CharacterController", flags);
                _fldInput = t.GetField("m_Input", flags);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Bhop] InitFields failed: {ex.Message}");
            }
        }

        public void OnUpdate()
        {
            if (_jumpDebounce > 0f)
            {
                _jumpDebounce -= Time.deltaTime;
            }

            var fpc = RM.fpsController;
            if (!Enabled || fpc == null)
            {
                CurrentSpeed = 0f;
                IsBhopping = false;
                return;
            }

            InitFields();

            // Cache vanilla physics parameters once
            if (_vanillaJumpSpeed < 0f && _fldJumpSpeed != null)
            {
                _vanillaJumpSpeed = (float)_fldJumpSpeed.GetValue(fpc);
                _vanillaGravityMultiplier = (float)_fldGravityMultiplier.GetValue(fpc);
            }

            // Apply custom Bhop JumpSpeed & GravityMultiplier to FirstPersonController
            if (_fldJumpSpeed != null)
            {
                _fldJumpSpeed.SetValue(fpc, JumpSpeed);
            }
            if (_fldGravityMultiplier != null)
            {
                _fldGravityMultiplier.SetValue(fpc, GravityMultiplier);
            }

            CharacterController cc = _fldCharController != null ? (CharacterController)_fldCharController.GetValue(fpc) : fpc.GetComponent<CharacterController>();
            bool isGrounded = cc != null && cc.isGrounded;
            bool isPressingJump = Input.GetKey(KeyCode.Space);

            // Auto Jump trigger
            if (AutoJump && isPressingJump && isGrounded && _jumpDebounce <= 0f)
            {
                DoJump(fpc, cc);
            }

            // Air & ground inertia physics
            if (!isGrounded && IsBhopping)
            {
                UpdateInertia();
            }
            else if (isGrounded)
            {
                if (!isPressingJump)
                {
                    float groundFric = _prefGroundFriction?.Value ?? 12f;
                    CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, 0f, groundFric * Time.deltaTime * 3f);
                    if (CurrentSpeed < 0.1f)
                    {
                        CurrentSpeed = 0f;
                        IsBhopping = false;
                    }
                }
                else
                {
                    bool hasMoveInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S);
                    if (!hasMoveInput && CurrentSpeed < 0.3f)
                    {
                        CurrentSpeed = 0f;
                        IsBhopping = false;
                    }
                }
            }
        }

        public void OnFixedUpdate()
        {
        }

        private void RestoreVanillaPhysics()
        {
            var fpc = RM.fpsController;
            if (fpc == null || _vanillaJumpSpeed < 0f) return;

            InitFields();
            _fldJumpSpeed?.SetValue(fpc, _vanillaJumpSpeed);
            _fldGravityMultiplier?.SetValue(fpc, _vanillaGravityMultiplier);
        }

        private void DoJump(FirstPersonController fpc, CharacterController cc)
        {
            if (fpc == null) return;

            _jumpDebounce = 0.08f;
            _fldJump?.SetValue(fpc, true);

            Vector3 v = cc != null ? cc.velocity : Vector3.zero;
            Vector3 horizontalVel = new Vector3(v.x, 0f, v.z);
            float currentHVel = horizontalVel.magnitude;

            Camera cam = Camera.main;
            Vector3 camForward = cam != null ? cam.transform.forward : fpc.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            bool hasMoveInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S);
            bool movingForward = currentHVel > 0.5f && Vector3.Dot(horizontalVel.normalized, camForward) > 0.1f;

            if (movingForward || (hasMoveInput && currentHVel > 0.3f))
            {
                IsBhopping = true;
                if (CurrentSpeed < currentHVel)
                {
                    CurrentSpeed = currentHVel;
                }
                else if (CurrentSpeed > 0.5f)
                {
                    CurrentSpeed *= 0.995f;
                }
            }
            else
            {
                // Jumping in place without movement input: keep speed at 0
                CurrentSpeed = 0f;
                IsBhopping = false;
            }
        }

        private void UpdateInertia()
        {
            float mouseX = Input.GetAxis("Mouse X");
            bool pressingA = Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D);
            bool pressingD = Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A);

            bool isStrafing = (pressingA && mouseX < -0.02f) || (pressingD && mouseX > 0.02f);

            float currentAcceleration = Acceleration;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentAcceleration *= BoostMultiplier;
            }

            if (isStrafing)
            {
                CurrentSpeed += currentAcceleration * Time.deltaTime;
            }
            else
            {
                if (CanLoseSpeed)
                {
                    CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, 0f, AirFriction * Time.deltaTime);
                }
            }
        }

        public void ApplyBhopVelocity(FirstPersonController fpc)
        {
            if (CurrentSpeed <= 0.1f || !IsBhopping || fpc == null) return;

            CharacterController cc = _fldCharController != null ? (CharacterController)_fldCharController.GetValue(fpc) : fpc.GetComponent<CharacterController>();
            if (cc == null || !cc.enabled) return;

            Camera cam = Camera.main;
            Vector3 forward = cam != null ? cam.transform.forward : fpc.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 motion = forward * CurrentSpeed * Time.fixedDeltaTime;
            cc.Move(motion);
        }

        public void ClearHorizontalInput(FirstPersonController fpc)
        {
            if (fpc != null && _fldInput != null)
            {
                _fldInput.SetValue(fpc, Vector2.zero);
            }
        }

        public void OnDrawSettings()
        {
            GUILayout.BeginVertical();

            // Auto-Jump
            bool autoJump = GUILayout.Toggle(AutoJump, "  Auto-Jump (Mantener Espacio)");
            if (autoJump != AutoJump) AutoJump = autoJump;

            // Show Speedometer
            bool showSpeedo = GUILayout.Toggle(ShowSpeedometer, "  Mostrar Velocímetro HUD");
            if (showSpeedo != ShowSpeedometer) ShowSpeedometer = showSpeedo;

            GUILayout.Space(8f);

            // Gravity Multiplier Slider
            GUILayout.Label($"Multiplicador de Gravedad: <color=#38BDF8><b>{GravityMultiplier:F1}x</b></color> <color=#9CA3AF>(evita flotar hacia el techo)</color>");
            float newGrav = GUILayout.HorizontalSlider(GravityMultiplier, 1.0f, 5.0f);
            if (Math.Abs(newGrav - GravityMultiplier) > 0.05f) GravityMultiplier = (float)Math.Round(newGrav, 1);

            GUILayout.Space(4f);

            // Jump Speed Slider
            GUILayout.Label($"Fuerza de Salto: <color=#38BDF8><b>{JumpSpeed:F1}</b></color> <color=#9CA3AF>(altura de salto baja/rápida)</color>");
            float newJump = GUILayout.HorizontalSlider(JumpSpeed, 2.0f, 9.0f);
            if (Math.Abs(newJump - JumpSpeed) > 0.05f) JumpSpeed = (float)Math.Round(newJump, 1);

            GUILayout.Space(4f);

            // Air Strafe Acceleration
            GUILayout.Label($"Aceleración de Strafe en Aire: <color=#00FFFF><b>{Acceleration:F1} m/s²</b></color>");
            float newAccel = GUILayout.HorizontalSlider(Acceleration, 0.5f, 8.0f);
            if (Math.Abs(newAccel - Acceleration) > 0.01f) Acceleration = (float)Math.Round(newAccel, 1);

            GUILayout.Space(4f);

            // Shift Boost Multiplier
            GUILayout.Label($"Multiplicador Shift (Sprint): <color=#FFD700><b>{BoostMultiplier:F1}x</b></color>");
            float newBoost = GUILayout.HorizontalSlider(BoostMultiplier, 1.0f, 6.0f);
            if (Math.Abs(newBoost - BoostMultiplier) > 0.01f) BoostMultiplier = (float)Math.Round(newBoost, 1);

            GUILayout.Space(4f);

            // Air Friction
            GUILayout.Label($"Fricción en Aire (Deceleración): <color=#AAAAAA><b>{AirFriction:F2}</b></color>");
            float newFriction = GUILayout.HorizontalSlider(AirFriction, 0.1f, 2.0f);
            if (Math.Abs(newFriction - AirFriction) > 0.01f) AirFriction = (float)Math.Round(newFriction, 2);

            GUILayout.Space(6f);
            GUILayout.Label("<color=#777777>Velocidad Máxima: Ilimitada / Uncapped (GoldSrc Style)</color>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 11 });

            GUILayout.EndVertical();
        }
    }
}
