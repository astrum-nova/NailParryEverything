using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace nailparryeverything;

public static class tweaks
{
    public static readonly Dictionary<string, string[]> parryableStates =  new()
    {
        { "Mossbone Mother", ["Swoop"] },
        { "Bone Flyer Giant", ["Charge", "Stomp"] },
        { "Last Judge", ["Stomp Down"] },
        { "Bone Beast", ["Charge", "Leap Out"] },
        { "Phantom", ["A Dash", "G Dash"] },
        { "Crawfather", ["Dive", "Peck Land"] },
        { "Bone Hunter Trapper", ["Charge", "Up Leap", "Diag Leap", "Fall"] },
        { "Conductor Boss", ["Charge"] },
        { "Slab Fly Broodmother", ["Leap"] },
        { "Roachkeeper Chef", ["Slam In", "Butt Air", "Stomp"] },
        { "Swamp Shaman", ["Dive"] },
        { "Hunter Queen Boss", ["Dash Grind"] },
        { "Vampire Gnat", ["Charge"] },
        { "Flower Queen Boss", ["Leap"] },
        { "Coral Conch Driller Giant Solo", ["Emerge", "Fly", "Impact"] },
        { "Driller B", ["Emerge", "Fly", "Impact"] },
        { "Driller A", ["Emerge", "Fly", "Impact"] },
    };
    public static readonly string[] whitelist =
    [
        "Dancer",
        "Tornado Event Sender",
        "Tormented Trobbio Tornado",
        "Drill Multihitter",
        "Conch Projectile Heavy",
        "lightning_rod_explode",
        "Ward Corpse Projectile",
        "Cloverstag White Sickle",
        "Slab Fly Glob",
        "Pollen Shot",
    ];

    public static readonly string[] blacklist =
    [
        "Spike Collider",
        "Splinter Queen Spike",
        "Splinter Queen Gate Spike",
        "Coral Crust Tree Plat",
    ];

    public static bool CheckWhitelist(Collider2D other)
    {
        var gameObject = other.gameObject;
        var parent = gameObject.transform.parent;
        var root = gameObject.transform.root;
        var res = false;
        if (!res && gameObject != null) res = whitelist.Any(whitelistedObject => other.gameObject.name.StartsWith(whitelistedObject));
        if (!res && parent != null) res = whitelist.Any(whitelistedObject => other.gameObject.transform.parent.name.StartsWith(whitelistedObject));
        if (!res && root != null) res = whitelist.Any(whitelistedObject => other.gameObject.transform.root.name.StartsWith(whitelistedObject));
        return res;
    }
    public static bool CheckBlacklist(Collider2D other)
    {
        var gameObject = other.gameObject;
        var parent = gameObject.transform.parent;
        var root = gameObject.transform.root;
        var res = false;
        if (!res && gameObject != null) res = blacklist.Any(whitelistedObject => other.gameObject.name.StartsWith(whitelistedObject));
        if (!res && parent != null) res = blacklist.Any(whitelistedObject => other.gameObject.transform.parent.name.StartsWith(whitelistedObject));
        if (!res && root != null) res = blacklist.Any(whitelistedObject => other.gameObject.transform.root.name.StartsWith(whitelistedObject));
        return res;
    }
    public static float HandleAdditionalIframes(GameObject gameObject)
    {
        var names = getGameObjectParentRootNames(gameObject);
        var gameObjectName = names[0];
        var parentName = names[1];
        var rootName = names[2];

        //* Cogwork Dancers
        if (gameObjectName.StartsWith("Dash Hit")) return 0.25f;
        //* Phantom
        if (parentName.StartsWith("Dragoon Blast") || parentName.StartsWith("Dragoon Down") || gameObjectName.StartsWith("Dragoon Down Damager")) return 0.25f;
        //* Widow
        if (rootName.StartsWith("Spinner_vertical_slash")) return 0.35f;
        //* Skull Tyrant
        if (rootName.StartsWith("Bone_Boulder SK")) return 0.35f;
        //* Raging Conchfly
        if (gameObjectName.StartsWith("Coral Conch Driller Roar")) return 0.35f;
        //* GrandMother Silk
        if (gameObjectName.StartsWith("SlashHit") && parentName.StartsWith("DashSlash Effect")) return 0.3f;
        //* Signis & Gron
        if (gameObjectName.StartsWith("Spear") && parentName.StartsWith("Stomp Colliders")) return 0.3f;
        //* Watcher at the edge
        if (parentName.StartsWith("sand_burst_effect_uppercut")) return 0.3f;
        //* Groal the Great
        if (gameObjectName.StartsWith("Swamp Shaman Fireball")) return 0.3f;
        //* Lost Garmond
        if (gameObjectName.StartsWith("Abyss Bullet")) return 0.3f;
        return 0;
    } 
    public static string[] getGameObjectParentRootNames(GameObject gameObject) => [gameObject == null ? "NULLGAMEOBJECT" : gameObject.name, gameObject.transform.parent ==  null ? "NULLPARENT" : gameObject.transform.parent.name,  gameObject.transform.root ==  null ? "NULLROOT" : gameObject.transform.root.name];

    public static bool IsValidActiveState(string bossName, string stateName) => parryableStates.TryGetValue(bossName, out var states) && states.Any(stateName.StartsWith);
}