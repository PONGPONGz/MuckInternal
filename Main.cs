using UnityEngine;
using System.Reflection;
using System.Collections;

namespace MuckInternal
{
    partial class Main : MonoBehaviour
    {
        private Menu menu = new Menu();

        private PlayerStatus statusInstance;
        private PlayerMovement movementInstance;

        private FieldInfo moveSpeedField;
        private FieldInfo hungerDrainRate;
        private FieldInfo staminaDrainRate;
        private FieldInfo jumpDrainRate;

        private float originalStaminaDrainRate;
        private float originalJumpDrainRate;
        private float originalHungerDrainRate;
        private int originalMaxHp;

        private void GetDefaultValues()
        {
            moveSpeedField   = movementInstance.GetType().GetField("maxRunSpeed",    BindingFlags.NonPublic | BindingFlags.Instance);
            staminaDrainRate = statusInstance.GetType().GetField("staminaDrainRate", BindingFlags.NonPublic | BindingFlags.Instance);
            hungerDrainRate  = statusInstance.GetType().GetField("hungerDrainRate",  BindingFlags.NonPublic | BindingFlags.Instance);
            jumpDrainRate    = statusInstance.GetType().GetField("jumpDrain",        BindingFlags.NonPublic | BindingFlags.Instance);

            originalStaminaDrainRate = (float)staminaDrainRate.GetValue(statusInstance);
            originalJumpDrainRate = (float)jumpDrainRate.GetValue(statusInstance);
            originalHungerDrainRate = (float)hungerDrainRate.GetValue(statusInstance);
            originalMaxHp = statusInstance.maxHp;
        }

        private void UpdateInstances()
        {
            statusInstance = PlayerStatus.Instance;
            movementInstance = PlayerMovement.Instance;
            GetDefaultValues();
        }

        public void Start()
        {
            Utils.CreateConsole();
        }

        // This function is called once per frame, it's frequency depends on the frame rate.
        // This is at the beginning of the game logic cycle.
        public void Update()
        {
            if (Input.GetKeyUp(KeyCode.Insert))
            {
                menu.Toggle();
            }
        }

        // This function is called once per frame, it's frequency depends on the frame rate.
        // This is at the end of the game logic cycle.
        public void LateUpdate()
        {
            if ((GameManager.state != GameManager.GameState.Playing) || !PlayerStatus.Instance || !PlayerMovement.Instance)
            {
                return;
            }

            if (PlayerStatus.Instance != statusInstance || PlayerMovement.Instance != movementInstance)
            {
                UpdateInstances();
            }

            if (menu.godmode && statusInstance.maxHp != 999)
            {
                originalMaxHp = statusInstance.maxHp;
                statusInstance.maxHp = 999;
            }
            else if (statusInstance.maxHp != originalMaxHp && !menu.godmode)
            {
                statusInstance.maxHp = originalMaxHp;
            }

            if (menu.godmode)
            {
                statusInstance.hp = statusInstance.maxHp;
                CurrentSettings.cameraShake = false;
            }

            if (menu.infiniteStamina && (float)staminaDrainRate.GetValue(statusInstance) != 0f)
            {
                originalStaminaDrainRate = (float)staminaDrainRate.GetValue(statusInstance);
                originalJumpDrainRate = (float)jumpDrainRate.GetValue(statusInstance);
                staminaDrainRate.SetValue(statusInstance, 0f);
                jumpDrainRate.SetValue(statusInstance, 0f);
            }
            else if (!menu.infiniteStamina && (float)staminaDrainRate.GetValue(statusInstance) != originalStaminaDrainRate)
            {
                staminaDrainRate.SetValue(statusInstance, originalStaminaDrainRate);
                jumpDrainRate.SetValue(statusInstance, originalJumpDrainRate);
            }

            if (menu.infiniteFood && (float)hungerDrainRate.GetValue(statusInstance) != 0f)
            {
                originalHungerDrainRate = (float)hungerDrainRate.GetValue(statusInstance);
                hungerDrainRate.SetValue(statusInstance, 0f);
            }
            else if (!menu.infiniteFood && (float)hungerDrainRate.GetValue(statusInstance) != originalHungerDrainRate)
            {
                hungerDrainRate.SetValue(statusInstance, originalHungerDrainRate);
            }

            if (menu.damage)
            {
                Hotbar.Instance.currentItem.resourceDamage = 10000;
                Hotbar.Instance.currentItem.attackDamage = 10000;
            }

            if (menu.moveSpeed != (float)moveSpeedField.GetValue(movementInstance))
            {
                moveSpeedField.SetValue(movementInstance, menu.moveSpeed);
            }
        }

        // This function is called at the end of the frame, after all game logic.
        // It is called twice per frame: Once for rendering, and once for GUI Events
        // This is where we do all drawing operations
        public void OnGUI()
        {
            if (menu.visible)
            {
                GUILayout.Window(0, Menu.WindowRect, menu.Render, Menu.Title);
            }
        }        
    }
}