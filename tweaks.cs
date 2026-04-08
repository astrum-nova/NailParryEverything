using System.Collections.Generic;
using UnityEngine;

namespace nailparryeverything;

public static class tweaks
{
    //? List of enemies and their unparryable states, sometimes an enemy's state starts with something from the parryable states list, but it turns out to be something like "Attack Recover" which should not be parryable
    private static readonly Dictionary<string, HashSet<string>> unparryableStates = new()
    {
        { "Pilgrim 01", ["Attack Recover"] },
        { "Pilgrim 03", ["Attack Recover"] },
        { "Mossbone Mother", ["Swoop Return", "Swoop Antic"] },
    };
    //? List of enemies and their parryable states
    private static readonly Dictionary<string, HashSet<string>> parryableStates = new() 
    {
        { "Mossbone Mother", ["Swoop"] },
        { "Mossbone Crawler Fat", ["Charge"] },
        { "Bone Flyer Giant", ["Charge", "Stomp"] },
        { "Last Judge", ["Stomp Down", "Charge"] },
        { "Bone Beast", ["Charge", "Leap Out", "Descending"] },
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
        { "Bone Flyer", ["Fire"] },
        { "Dancer A", ["Stomp"] },
        { "Dancer B", ["Stomp"] },
        { "MossBone Fly", ["Drill"] },
        { "Bone Roller", ["CCW", "CW"] },
        { "Bone Goomba Large", ["Charge"] },
        { "Pilgrim 01", ["Attack"] },
        { "Pilgrim 03", ["Attack"] },
        { "Pilgrim Fly", ["Charge"] },
        { "Aspid Collector", ["Chomp"] },
        { "Bone Circler", ["Chase"] },
        { "Rosary Thief", ["Dash Air"] },
        { "Bone Thumper", ["Roll"] },
        { "Shellwood Goomba Flyer", ["Chase"] },
        { "Bloom Puncher", ["Punch"] },
        { "Bone Crawler", ["Spike"] },
    };
    //? Objects that can be parried, sometimes its impossible to check if something is an attack so we have a list for it
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
        "Stomp Blast",
        "Stomp Colliders",
        "Collider Slam",
        "DashCollider",
        "Crawfather Attack Chain"
    ];
    //? Objects that cannot be parried, sometimes objects like hazards are considered attacks and they should not be parried 
    private static readonly HashSet<string> blacklist =
    [
        "Spike Collider",
        "Splinter Queen Spike",
        "Splinter Queen Gate Spike",
        "Coral Crust Tree Plat",
        "Battle Gate Coral",
        "Head Collider"
    ];
    //? Enemies that can be attacked directly
    private static readonly HashSet<string> fragileList =
    [
        "Shellwood Gnat",
        "Shellwood Goomba",
        "Bone Goomba",
        "Slab Fly Small Fresh",
        "MossBone Cocoon",
        "MossBone Crawler",
    ];
    //? If an enemy's name starts with something from the fragile list we do an additional check to make sure it a tougher version of that enemy that can be parried, like the goombas
    private static readonly HashSet<string> tuffList =
    [
        "Bone Goomba Large",
        "MossBone Crawler Fat",
    ];
    public static bool CheckList(Collider2D other, int listId)
    {
        var gameObject = other.gameObject;
        var parent = gameObject.transform.parent;
        var root = gameObject.transform.root;
        
        //? Harnessing 1% team cherrys power to create abominations of code 
        foreach (var entry in listId switch
                 {
                     0 => blacklist,
                     1 => whitelist,
                     2 => fragileList,
                     _ => tuffList
                 })
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
        var unparryable = unparryableStates.TryGetValue(bossName, out var unpstates) && unpstates.Contains(stateName);
        foreach (var state in states) if (stateName.StartsWith(state) && !unparryable) return true;
        return false;
    }
}