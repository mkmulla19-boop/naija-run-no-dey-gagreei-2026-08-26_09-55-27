using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Olomu.Systems
{
    public class PromoCapture : MonoBehaviour
    {
        private string outDir = @"C:\ProgramData\olomu_promo";

        private void Start()
        {
            var args = System.Environment.GetCommandLineArgs();
            bool promo = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-promo") promo = true;
                if (args[i] == "-promoOut" && i + 1 < args.Length) outDir = args[i + 1];
            }
            if (!promo)
            {
                enabled = false;
                return;
            }
            Directory.CreateDirectory(outDir);
            Screen.SetResolution(1920, 1080, false);
            Application.runInBackground = true;
            Time.captureFramerate = 30;
            StartCoroutine(Run());
            StartCoroutine(Watchdog());
        }

        private IEnumerator Watchdog()
        {
            int lastFrame = -1;
            float lastProgress = Time.realtimeSinceStartup;
            while (true)
            {
                yield return new WaitForSecondsRealtime(20f);
                if (frameCount != lastFrame)
                {
                    lastFrame = frameCount;
                    lastProgress = Time.realtimeSinceStartup;
                }
                else if (Time.realtimeSinceStartup - lastProgress > 150f)
                {
                    File.WriteAllText(Path.Combine(outDir, "DONE"), frameCount + "_watchdog");
                    Application.Quit();
                    yield break;
                }
            }
        }

        private int frameCount;

        private IEnumerator Run()
        {
            yield return new WaitForSeconds(0.5f);

            int frame = 0;
            bool finished = false;
            var director = FindFirstObjectByType<CinematicDirector>();
            File.WriteAllText(Path.Combine(outDir, "diag.txt"),
                "director=" + (director != null) + "\n");
            if (director != null)
                director.Finished += () => { finished = true; };
            else
                finished = true;

            Transform playerT = null;
            ThirdPersonController ctl = null;
            Interactor inter = null;
            Camera orbit = null;
            float orbitAngle = 0f;
            float combatT = 0f;
            bool combatStarted = false;
            var spawned = new List<GameObject>();
            float attackClock = 0f;

            while (!(finished && combatT > 24f))
            {
                if (!combatStarted && director != null &&
                    director.Current == CinematicDirector.Phase.Playing)
                    finished = true;

                if (frame % 60 == 0)
                    File.AppendAllText(Path.Combine(outDir, "diag.txt"),
                        "f=" + frame + " finished=" + finished +
                        " phase=" + (director != null ? director.Current.ToString() : "n/a") + "\n");

                if (finished && !combatStarted)
                {
                    combatStarted = true;
                    var pc = FindFirstObjectByType<ThirdPersonController>();
                    if (pc != null)
                    {
                        ctl = pc;
                        playerT = pc.transform;
                        inter = pc.GetComponent<Interactor>();
                        var gameCam = pc.GetComponentInChildren<Camera>(true);
                        if (gameCam != null) gameCam.gameObject.SetActive(false);
                    }
                    var orbitGo = new GameObject("OrbitCam");
                    orbit = orbitGo.AddComponent<Camera>();
                    orbit.fieldOfView = 50f;
                    orbit.farClipPlane = 500f;
                    orbit.depth = 10f;
                    orbit.tag = "Untagged";
                    SpawnRaider(playerT, spawned);
                    SpawnRaider(playerT, spawned);
                }

                if (combatStarted && playerT != null)
                {
                    combatT += Time.deltaTime;

                    spawned.RemoveAll(g => g == null);
                    if (spawned.Count < 2 && combatT < 19f)
                        SpawnRaider(playerT, spawned);

                    GameObject nearest = null;
                    float bestSqr = float.MaxValue;
                    foreach (var g in spawned)
                    {
                        float d = (g.transform.position - playerT.position).sqrMagnitude;
                        if (d < bestSqr) { bestSqr = d; nearest = g; }
                    }

                    if (nearest != null)
                    {
                        Vector3 flat = nearest.transform.position - playerT.position;
                        flat.y = 0f;
                        float dist = flat.magnitude;
                        float yaw = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;
                        ctl.SetYaw(yaw);
                        if (dist > 1.9f)
                            ctl.SetCinematicInput(new Vector2(0f, 1f));
                        else
                        {
                            ctl.SetCinematicInput(Vector2.zero);
                            attackClock -= Time.deltaTime;
                            if (attackClock <= 0f && inter != null)
                            {
                                inter.TryAttack();
                                attackClock = 0.7f;
                            }
                        }
                    }

                    orbitAngle += 38f * Time.deltaTime;
                    float rad = orbitAngle * Mathf.Deg2Rad;
                    Vector3 pos = playerT.position +
                        new Vector3(Mathf.Sin(rad) * 5.4f, 2.5f, Mathf.Cos(rad) * 5.4f);
                    orbit.transform.position = pos;
                    orbit.transform.rotation = Quaternion.LookRotation(
                        playerT.position + Vector3.up * 1.25f - pos, Vector3.up);
                }

                ScreenCapture.CaptureScreenshot(
                    Path.Combine(outDir, "frame_" + frame.ToString("D5") + ".png"));
                frameCount = frame;
                frame++;
                yield return new WaitForEndOfFrame();
            }

            File.WriteAllText(Path.Combine(outDir, "DONE"), frame.ToString());
            Application.Quit();
        }

        private static void SpawnRaider(Transform playerT, List<GameObject> list)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "DemoRaider";
            Vector2 r = Random.insideUnitCircle.normalized * Random.Range(3.5f, 5.5f);
            go.transform.position = playerT.position + new Vector3(r.x, 1.05f, r.y);
            go.transform.localScale = new Vector3(0.85f, 1.1f, 0.85f);
            var cc = go.AddComponent<CharacterController>();
            cc.height = 2.1f;
            var ai = go.AddComponent<EnemyAI>();
            ai.health = 100f;
            ai.attackDamage = 6f;
            ai.attackCooldown = 1.4f;
            ai.chaseSpeed = 5.4f;
            ai.detectRadius = 40f;
            ai.lootItem = "hide";
            list.Add(go);
        }
    }
}
