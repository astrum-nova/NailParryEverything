using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace nailparryeverything;

public static class tweaks
{
    public static readonly Dictionary<string, HashSet<string>> parryableStates = new() 
    {
        { "Mossbone Mother", ["Swoop"] },
        { "Bone Flyer Giant", ["Charge", "Stomp"] },
        { "Last Judge", ["Stomp Down", "Charge"] },
        { "Bone Beast", ["Charge", "Leap Out"] },
        { "Phantom", ["A Dash", "G Dash", "Dragoon Down"] },
        { "Crawfather", ["Dive", "Peck Land"] },
        { "Bone Hunter Trapper", ["Charge", "Up Leap", "Diag Leap", "Fall"] },
        { "Conductor Boss", ["Charge"] },
        { "Slab Fly Broodmother", ["Leap", "Slam"] },
        { "Roachkeeper Chef", ["Slam In", "Butt Air", "Stomp", "Dive"] },
        { "Swamp Shaman", ["Dive"] },
        { "Hunter Queen Boss", ["Dash Grind"] },
        { "Vampire Gnat", ["Charge"] },
        { "Flower Queen Boss", ["Leap"] },
        { "Coral Conch Driller Giant Solo", ["Emerge", "Fly", "Impact"] },
        { "Driller B", ["Emerge", "Fly", "Impact"] },
        { "Driller A", ["Emerge", "Fly", "Impact"] },
        { "Skull King", ["Charge", "Rewake Drop", "Stomp Jump", "Jump Air"] },
        { "Trobbio", ["Tornado"] },
        { "Tormented Trobbio", ["Tornado"] },
        { "Seth", ["Jump Dive"] },
        { "Song Reed Grand", ["Stomp", "Cast"] },
        { "Slab Fly Large", ["Stomp", "Grab Air", "Roll"] },
        { "Spinner Boss", ["Charge"] },
        { "Giant Flea", ["Dashing"] },
        { "Dock Guard Thrower", ["Stomp"] },
        { "Bone Hunter Fly", ["Dthrust"] },
        { "Bone Hunter Fly Chief", ["Dthrust"] },
        { "Bone Flyer Smn", ["Fire"] },
        { "Dancer A", ["Stomp"] },
        { "Dancer B", ["Stomp"] },
    };
    private static readonly HashSet<string> whitelist =
    [
        "Tornado Event Sender",
        "Tormented Trobbio Tornado",
        "Drill Multihitter",
        "Conch Projectile Heavy",
        "lightning_rod_explode",
        "Ward Corpse Projectile",
        "Cloverstag White Sickle",
        "Slab Fly Glob",
        "Pollen Shot",
        "Slab Fly Small Fresh",
        "Stomp Blast",
        "Stomp Colliders",
        "Collider Slam",
        "DashCollider",
        "Crawfather Attack Chain"
    ];
    private static readonly HashSet<string> blacklist =
    [
        "Spike Collider",
        "Splinter Queen Spike",
        "Splinter Queen Gate Spike",
        "Coral Crust Tree Plat",
        "Battle Gate Coral"
    ];
    public static bool CheckList(Collider2D other, bool whiteOrBlackList)
    {
        var gameObject = other.gameObject;
        var parent = gameObject.transform.parent;
        var root = gameObject.transform.root;
        foreach (var entry in whiteOrBlackList ? whitelist : blacklist)
        {
            if (gameObject != null && gameObject.name.StartsWith(entry)) return true;
            if (parent != null && parent.name.StartsWith(entry)) return true;
            if (root != null && root.name.StartsWith(entry)) return true;
        }
        return false;
    }
    public static float HandleAdditionalIframes(GameObject gameObject)
    {
        if (gameObject == null) return 0;
        var gameObjectName = gameObject.name;
        var parentName = gameObject.transform.parent != null ? gameObject.transform.parent.name : "";
        var rootName = gameObject.transform.root != null ? gameObject.transform.root.name : "";
        if (gameObjectName.StartsWith("Dash Hit")) return 0.25f; //* Cogwork Dancers
        if (parentName.StartsWith("Dragoon Blast") || parentName.StartsWith("Dragoon Down") || gameObjectName.StartsWith("Dragoon Down")) return 0.25f; //* Phantom
        if (rootName.StartsWith("Spinner_vertical_slash")) return 0.35f; //* Widow
        if (rootName.StartsWith("Bone_Boulder SK")) return 0.35f; //* Skull Tyrant
        if (gameObjectName.StartsWith("Coral Conch Driller Roar")) return 0.35f; //* Raging Conchfly
        if (gameObjectName.StartsWith("SlashHit") && parentName.StartsWith("DashSlash Effect")) return 0.3f; //* GrandMother Silk
        if ((gameObjectName.StartsWith("Spear") || gameObjectName.StartsWith("Body")) && parentName.StartsWith("Stomp Colliders")) return 0.3f; //* Signis & Gron
        if (parentName.StartsWith("sand_burst_effect_uppercut")) return 0.3f; //* Watcher at the edge
        if (gameObjectName.StartsWith("Swamp Shaman Fireball")) return 0.3f; //* Groal the Great
        if (gameObjectName.StartsWith("Abyss Bullet")) return 0.3f; //* Lost Garmond
        return 0;
    }
    
    //! DEBUG !\\
    public static string[] getGameObjectParentRootNames(GameObject gameObject) => [gameObject == null ? "NULLGAMEOBJECT" : gameObject.name, gameObject.transform.parent ==  null ? "NULLPARENT" : gameObject.transform.parent.name,  gameObject.transform.root ==  null ? "NULLROOT" : gameObject.transform.root.name];
    //! DEBUG !\\
    
    public static bool IsValidActiveState(string bossName, string stateName)
    {
        if (!parryableStates.TryGetValue(bossName, out var states)) return false;
        foreach (var state in states) if (stateName.StartsWith(state)) return true;
        return false;
    }
}