using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using Excel.Log;
using UnityEngine;
using UnityEngine.UI;

namespace BadCirno;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin: BaseUnityPlugin
{
    public static ManualLogSource Log => BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_NAME);
    
    public string pluginInfo = $"{MyPluginInfo.PLUGIN_NAME} By MetaMiku v{MyPluginInfo.PLUGIN_VERSION}";
    public string repoInfo = "https://github.com/MetaMystia/BadCirno";
    
    // Bad Apple Variables
    private bool isPlayingBadApple = false;
    private float badAppleTimer = 0f;
    private int badAppleFrameIndex = 0;
    private const float FRAME_INTERVAL = 1.0f / 15.0f; // 15 fps

    void Start()
    {
        Log.LogWarning(pluginInfo);
        Log.LogWarning(repoInfo);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ClearBullets();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ToggleBadApple();
        }
        
        if (isPlayingBadApple)
        {
            UpdateBadApple();
        }
    }

    void ToggleBadApple()
    {
        isPlayingBadApple = !isPlayingBadApple;
        if (isPlayingBadApple)
        {
            badAppleTimer = FRAME_INTERVAL;
            badAppleFrameIndex = 0;
            Logger.LogInfo("Bad Apple Started");
        }
        else
        {
            ClearBullets();
            Logger.LogInfo("Bad Apple Stopped");
        }
    }

    void UpdateBadApple()
    {
        badAppleTimer += Time.deltaTime;
        if (badAppleTimer >= FRAME_INTERVAL)
        {
            badAppleTimer -= FRAME_INTERVAL;
            PlayNextFrame();
        }
    }

    void PlayNextFrame()
    {
        if (badAppleFrameIndex >= BadAppleData.Data.Length)
        {
            isPlayingBadApple = false;
            ClearBullets();
            return;
        }

        ClearBullets();

        var frame = BadAppleData.Data[badAppleFrameIndex];
        SpawnFrameBullets(frame);
        
        badAppleFrameIndex++;
    }

    void SpawnFrameBullets(bool[,] frame)
    {
        if (Player.Instance == null) return;
        Vector3 center = Player.Instance.transform.position;
        
        // 16x12 grid
        float spacing = 0.5f;
        float startX = 0 - (16 * spacing) / 2.0f;
        float startY = 3 + (12 * spacing) / 2.0f; 
        
        for (int row = 0; row < 12; row++)
        {
            for (int col = 0; col < 16; col++)
            {
                if (frame[row, col])
                {
                    Vector3 pos = center + new Vector3(startX + col * spacing, startY - row * spacing, 0);
                    SpawnBulletAt(pos, Quaternion.identity, Vector2.zero);
                }
            }
        }
    }

    void SpawnBulletAt(Vector3 pos, Quaternion rotation, Vector2 velocity)
    {
        AssetUtil.SpawnBulletSync(EnumBullet.A90009, bullet =>
        {
            if (bullet == null) return;
            
            bullet.transform.position = pos;
            bullet.transform.rotation = rotation;
            bullet.transform.Scale(0.5f);
            
            bullet.destoryEffectId = EnumEffect.None;
            bullet.destoryClipId = EnumAudioClip.None;

            bullet.CanHitTarget = false;
            
            bullet.Rigidbody2D.gravityScale = 0f;
            bullet.Rigidbody2D.linearVelocity = velocity;
        });
    }

    void ClearBullets()
    {
        BaseSingleTon<EventManager>.Instance.ExecuteEvent(EventId.DesAllBullets);
    }
}