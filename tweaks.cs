using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace nailparryeverything;

public class tweaks
{
    public static Dictionary<string, float> parryableStates =  new()
    {
        
    };

    public static string[] whitelist =
    [
        "Dancer",
        "Tornado Event Sender",
    ];

    public static string[] blacklist =
    [
        "Spike Collider",
        "Splinter Queen Spike",
        "Splinter Queen Gate Spike",
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
}