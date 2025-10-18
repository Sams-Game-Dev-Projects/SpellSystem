using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using SpellSystem;

// Tools menu that auto-creates sample content and a demo scene.
public static class SpellSystemSetup
{
    private const string Root = "Assets/SpellSystem";
    private const string Prefabs = Root + "/Prefabs";
    private const string Spells = Root + "/Spells";
    private const string VFX = Root + "/VFX";
    private const string Materials = Root + "/Materials";
    private const string InputPath = "Assets/Input";

    [MenuItem("Tools/Spell System/Create Sample Content")] 
    public static void CreateSampleContent()
    {
        EnsureFolders();

        // Materials
        var redMat = CreateColorMaterial("Fire_Red", new Color(1f, 0.25f, 0.1f));
        var purpleMat = CreateColorMaterial("Eldritch_Purple", new Color(0.5f, 0.2f, 0.8f));
        var greenMat = CreateColorMaterial("Heal_Green", new Color(0.2f, 0.9f, 0.3f));

        // VFX prefabs
        var healCastVFX = CreateSimpleParticlePrefab("HealCastVFX", greenMat.color, 1.2f, 20, 0.5f);
        var regenVFX = CreateSimpleParticlePrefab("RegenVFX", new Color(0.1f, 1f, 0.6f), 1.2f, 15, 1.5f);
        var impactFireVFX = CreateBurstParticlePrefab("ImpactFireVFX", redMat.color, 30);
        var impactEldritchVFX = CreateBurstParticlePrefab("ImpactEldritchVFX", purpleMat.color, 24);

        // Projectile prefabs
        var fireball = CreateSphereProjectilePrefab("FireballProjectile", redMat, 0.3f);
        var eldritch = CreateSphereProjectilePrefab("EldritchBlastProjectile", purpleMat, 0.25f);

        // Spell assets
        CreateSpellAsset("Self_Heal_Instant", s =>
        {
            s.displayName = "Instant Heal";
            s.description = "Immediately restores health to the caster.";
            s.targeting = SpellTargeting.Self;
            s.type = DamageType.Holy;
            s.isHealing = true;
            s.manaCost = 20;
            s.delivery = EffectDelivery.Instant;
            s.amount = 50f;
            s.castVFXPrefab = healCastVFX;
        });

        CreateSpellAsset("Self_Heal_Regen", s =>
        {
            s.displayName = "Heal Over Time";
            s.description = "Regenerate health over a short duration.";
            s.targeting = SpellTargeting.Self;
            s.type = DamageType.Nature;
            s.isHealing = true;
            s.manaCost = 15;
            s.delivery = EffectDelivery.OverTime;
            s.amount = 50f;
            s.duration = 5f;
            s.tickInterval = 1f;
            s.castVFXPrefab = regenVFX;
        });

        CreateSpellAsset("Fireball_DoT", s =>
        {
            s.displayName = "Fireball (DoT)";
            s.description = "Launch a fireball that burns enemies over time.";
            s.targeting = SpellTargeting.Projectile;
            s.type = DamageType.Fire;
            s.isHealing = false;
            s.manaCost = 10;
            s.delivery = EffectDelivery.OverTime;
            s.amount = 30f; // total over duration
            s.duration = 3f;
            s.tickInterval = 0.5f;
            s.projectilePrefab = fireball;
            s.projectileSpeed = 18f;
            s.projectileLifetime = 5f;
            s.impactVFXPrefab = impactFireVFX;
        });

        CreateSpellAsset("EldritchBlast_Instant", s =>
        {
            s.displayName = "Eldritch Blast";
            s.description = "Launch a bolt that deals force damage instantly.";
            s.targeting = SpellTargeting.Projectile;
            s.type = DamageType.Force;
            s.isHealing = false;
            s.manaCost = 12;
            s.delivery = EffectDelivery.Instant;
            s.amount = 40f;
            s.projectilePrefab = eldritch;
            s.projectileSpeed = 24f;
            s.projectileLifetime = 4f;
            s.impactVFXPrefab = impactEldritchVFX;
        });

        Debug.Log("[Spell System] Sample content created under " + Root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

 

    [MenuItem("Tools/Spell System/Setup Sample Scene")]
    public static void SetupSampleScene()
    {
        EnsureFolders();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(4, 1, 4);

        // Player
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0, 1, -6);
        player.AddComponent<CharacterHealth>();
        player.AddComponent<ManaPool>();
        player.AddComponent<SpellCaster>();
        player.AddComponent<CharacterController>();
        var controller = player.AddComponent<PlayerController>();

#if ENABLE_INPUT_SYSTEM
        // PlayerInput wired to our generated asset if present.
        var inputAsset = AssetDatabase.LoadAssetAtPath<Object>(InputPath + "/SpellSystem_Controls.inputactions") as ScriptableObject;
        var pi = player.AddComponent<UnityEngine.InputSystem.PlayerInput>();
        pi.notificationBehavior = UnityEngine.InputSystem.PlayerNotifications.InvokeUnityEvents;
        if (inputAsset != null)
        {
            var so = new SerializedObject(pi);
            so.FindProperty("m_Actions").objectReferenceValue = inputAsset;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
#endif

        // Enemies
        for (int i = 0; i < 3; i++)
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.name = "Enemy" + (i + 1);
            enemy.transform.position = new Vector3(-4 + i * 4, 0.5f, 6);
            enemy.AddComponent<CharacterHealth>();
        }

        // Assign sample spells if they exist
        var caster = player.GetComponent<SpellCaster>();
        var healInstant = AssetDatabase.LoadAssetAtPath<SpellDefinition>(Spells + "/Self_Heal_Instant.asset");
        var healRegen = AssetDatabase.LoadAssetAtPath<SpellDefinition>(Spells + "/Self_Heal_Regen.asset");
        var fireball = AssetDatabase.LoadAssetAtPath<SpellDefinition>(Spells + "/Fireball_DoT.asset");
        var eldritch = AssetDatabase.LoadAssetAtPath<SpellDefinition>(Spells + "/EldritchBlast_Instant.asset");
        if (caster != null)
        {
            caster.spellSlots = new SpellDefinition[4] { healInstant, healRegen, fireball, eldritch };
        }

        Debug.Log("[Spell System] Sample scene created. Assign Input Actions and bind Unity Events for Move/Spell1-4 if needed.");
    }

    // Helpers
    private static void EnsureFolders()
    {
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(Prefabs);
        Directory.CreateDirectory(Spells);
        Directory.CreateDirectory(VFX);
        Directory.CreateDirectory(Materials);
        Directory.CreateDirectory(InputPath);
    }

    private static Material CreateColorMaterial(string name, Color color)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        AssetDatabase.CreateAsset(mat, Materials + "/" + name + ".mat");
        return mat;
    }

    private static GameObject CreateSphereProjectilePrefab(string name, Material mat, float scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.localScale = Vector3.one * scale;
        if (mat) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        var proj = go.AddComponent<SpellProjectile>();
        var col = go.GetComponent<SphereCollider>();
        col.isTrigger = true;
        var rb = go.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        var prefabPath = Prefabs + "/" + name + ".prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateSimpleParticlePrefab(string name, Color color, float size, int count, float lifetime)
    {
        var go = new GameObject(name);
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main; 
        main.startColor = color; 
        main.startSize = size; 
        main.startLifetime = lifetime; 
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false; 
        main.stopAction = ParticleSystemStopAction.Destroy;

        var em = ps.emission; 
        em.rateOverTime = 0f; 
        var burst = new ParticleSystem.Burst(0f, (short)Mathf.Max(1, count));
        em.SetBursts(new[] { burst });
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 0.2f;
        var prefabPath = VFX + "/" + name + ".prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject CreateBurstParticlePrefab(string name, Color color, int burstCount)
    {
        var go = new GameObject(name);
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main; 
        main.startColor = color; 
        main.startSize = 0.3f; 
        main.startLifetime = 1.2f; 
        main.loop = false; 
        main.stopAction = ParticleSystemStopAction.Destroy;
        var em = ps.emission; em.rateOverTime = 0f; var burst = new ParticleSystem.Burst(0f, (short)burstCount); em.SetBursts(new[] { burst });
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 0.1f;
        var prefabPath = VFX + "/" + name + ".prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void CreateSpellAsset(string fileName, System.Action<SpellDefinition> configure)
    {
        var asset = ScriptableObject.CreateInstance<SpellDefinition>();
        configure?.Invoke(asset);
        AssetDatabase.CreateAsset(asset, Spells + "/" + fileName + ".asset");
    }
}
