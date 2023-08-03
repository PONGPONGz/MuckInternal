using System.Text.RegularExpressions;
using System;
using UnityEngine;
using Mono.Math;
using System.Reflection;
using System.Collections.Generic;

namespace MuckInternal
{
    internal class Menu
    {
        private static readonly int WindowWidth = 700, WindowHeight = 400;
        public static readonly Rect WindowRect = new Rect((Screen.width - WindowWidth) / 2, (Screen.height - WindowHeight) / 2, WindowWidth, WindowHeight);
        private static readonly float MenuWidth = (WindowWidth - 20);
        private static readonly int boxWidth = 60, padding = 5;
        private static readonly int cellsPerRow = (int)((MenuWidth * 0.7f) / boxWidth);
        public static readonly string Title = "github.com/PONGPONGzy";

        private static float DefaultRunningSpeed = 13.0f;
        private static float MaxRunningSpeed = 50.0f;
        private static int DefaultSpawnAmount = 1;

        public bool visible = false;

        public bool godmode         = false;
        public bool infiniteStamina = false;
        public bool infiniteFood    = false;
        public bool damage          = false;

        public float moveSpeed = DefaultRunningSpeed;

        private int elementCount = 0;
        private Vector2 scrollPos = new Vector2(20, 0);
        private string searchText = "";
        private string amountText = DefaultSpawnAmount.ToString();
        private int spawnAmount { 
            get 
            {
                int retval;
                if (!int.TryParse(amountText, out retval))
                {
                    retval = DefaultSpawnAmount;
                    amountText = retval.ToString();
                }
                return retval; 
            } 
        }

        private CursorLockMode originalLockMode;
        private bool originalVisibility;

        public void Toggle()
        {
            visible = !visible;
            if (visible)
            {
                originalLockMode = Cursor.lockState;
                originalVisibility = Cursor.visible;
            }

            Cursor.visible = visible || originalVisibility;
            Cursor.lockState = visible ? CursorLockMode.None : originalLockMode;
        }

        public void Render(int windowId)
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginArea(new Rect(10, 20, MenuWidth * 0.3f, 400));
            DrawOptions();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(MenuWidth * 0.3f + 20, 20, MenuWidth * 0.7f, 370));
            LoadSpawnableObjects();
            GUILayout.EndArea();

            GUILayout.EndHorizontal();
        }

        private void AddSpawnButton<T>(T element, GUIContent content, Action<T> callback)
        {
            int currentColumn = (elementCount % cellsPerRow);
            int currentRow = (int)(elementCount / cellsPerRow);
            if (GUI.Button(new Rect(padding + ((boxWidth + padding) * currentColumn), currentRow * (boxWidth + padding), boxWidth, boxWidth), content))
                callback(element);
            elementCount++;
        }

        private void DisplaySpawnableObjects<T>(T[] elements, Func<T, GUIContent> getContent, Action<T> callback)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                T element = elements[i];
                GUIContent content = getContent(element);
                if (content.tooltip.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                AddSpawnButton(element, content, callback);
            }
        }

        private void LoadSpawnableObjects()
        {
            GUILayout.Box("Spawner", GUILayout.Width(MenuWidth * 0.7f - 20));
            GUILayout.BeginHorizontal(GUILayout.Width(MenuWidth * 0.7f - 20 - padding));

            GUILayout.Label("Search:");
            searchText = GUILayout.TextField(searchText, GUILayout.Width((MenuWidth * 0.55f) / 2));

            GUILayout.FlexibleSpace();

            GUILayout.Label("Amount:");
            amountText = GUILayout.TextField(amountText, 2, GUILayout.Width((MenuWidth * 0.068f) / 2));
            amountText = Regex.Replace(amountText, @"[^\d]+", "");

            GUILayout.EndHorizontal();
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            elementCount = 0;
            if (ItemManager.Instance && MobSpawner.Instance)
            {
                DisplaySpawnableObjects(ItemManager.Instance.allScriptableItems,
                    item => new GUIContent(item.sprite.texture, item.name),
                    item => ItemManager.Instance.DropItemAtPosition(item.id, spawnAmount, PlayerMovement.Instance.transform.position, ItemManager.Instance.GetNextId())
                );

                DisplaySpawnableObjects(MobSpawner.Instance.mobsInspector,
                    mob => new GUIContent(mob.name, mob.name),
                    mob => MobSpawner.Instance.ServerSpawnNewMob(MobManager.Instance.GetNextId(), mob.id, PlayerMovement.Instance.transform.position, 1f, 1f, Mob.BossType.None, -1)
                );
            }

            int height = (int)Math.Ceiling((decimal)((elementCount / cellsPerRow) + 0.5));      // add 0.5 to avoid double rounding error https://stackoverflow.com/a/8833131/15220619
            GUILayout.Label("", GUILayout.Height((boxWidth + padding) * height));
            GUILayout.EndScrollView();
        }

        private void DrawOptions()
        {
            GUILayout.Box("Options", GUILayout.Width(MenuWidth * 0.3f));

            godmode         = GUILayout.Toggle(godmode,         " Godmode");
            infiniteStamina = GUILayout.Toggle(infiniteStamina, " Infinite Stamina");
            infiniteFood    = GUILayout.Toggle(infiniteFood,    " Infinite Food");
            damage          = GUILayout.Toggle(damage,          " Infinite Damage");

            GUILayout.Label("Run Speed: " + moveSpeed.ToString());
            moveSpeed = GUILayout.HorizontalSlider(moveSpeed, DefaultRunningSpeed, MaxRunningSpeed);

            if (GUILayout.Button("KILL ALL MOBS"))
            {
                foreach (Mob mob in GameObject.FindObjectsOfType<Mob>())
                    mob.hitable.Damage(0, LocalClient.instance.myId, (int)HitEffect.Normal, Vector3.zero);
            }

            if (GUILayout.Button("BREAK ALL TREES"))
            {
                foreach (HitableTree tree in GameObject.FindObjectsOfType<HitableTree>())
                    tree.Hit(tree.hp, 9999, (int)HitEffect.Normal, Vector3.zero, 0);
            }

            if (GUILayout.Button("BREAK ALL ROCKS"))
            {
                foreach (HitableRock rock in GameObject.FindObjectsOfType<HitableRock>())
                    rock.Hit(rock.hp, 9999, (int)HitEffect.Normal, Vector3.zero, 0);
            }
        }
    }
}
