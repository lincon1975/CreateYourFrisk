﻿using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public static class SpriteFontRegistry {
    public const string UI_DEFAULT_NAME = "uidialog";
    public const string UI_DAMAGETEXT_NAME = "uidamagetext";
    public const string UI_MONSTERTEXT_NAME = "monster";
    public const string UI_SMALLTEXT_NAME = "uibattlesmall";

    public static GameObject LETTER_OBJECT;
    public static GameObject BUBBLE_OBJECT;
    private static Dictionary<string, FileInfo> dictDefault = new Dictionary<string, FileInfo>();
    private static Dictionary<string, FileInfo> dictMod = new Dictionary<string, FileInfo>();

    private static Dictionary<string, UnderFont> dict = new Dictionary<string, UnderFont>();
    //private static bool initialized;

    public static void Start() {
        loadAllFrom(FileLoader.pathToDefaultFile("Sprites/UI/Fonts"));
    }

    public static UnderFont Get(string key) {
        string k = key;
        key = key.ToLower();
        if (dict.ContainsKey(key))
            return dict[key];
        else
            return tryLoad(k);
        //return null;
    }

    public static void init() {
        dict.Clear();
        /*if (initialized)
            return;*/
        LETTER_OBJECT = Resources.Load("Fonts/letter") as GameObject;
        BUBBLE_OBJECT = Resources.Load("Prefabs/DialogBubble") as GameObject;

        //string modPath = FileLoader.pathToModFile("Sprites/UI/Fonts");
        //string defaultPath = FileLoader.pathToDefaultFile("Sprites/UI/Fonts");
        //loadAllFrom(defaultPath);
        loadAllFrom(FileLoader.pathToModFile("Sprites/UI/Fonts"), true);

        //initialized = true;
    }

    private static void loadAllFrom(string directoryPath, bool mod = false) {
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
        if (!dInfo.Exists)
            return;
        FileInfo[] fInfo = dInfo.GetFiles("*.png", SearchOption.TopDirectoryOnly);

        if (mod) {
            dictMod.Clear();
            foreach (FileInfo file in fInfo)
                dictMod[Path.GetFileNameWithoutExtension(file.FullName).ToLower()] = file;
        } else {
            dictDefault.Clear();
            foreach (FileInfo file in fInfo)
                dictDefault[Path.GetFileNameWithoutExtension(file.FullName).ToLower()] = file;
        }
        /*foreach (FileInfo file in fInfo) {
            string fontName = Path.GetFileNameWithoutExtension(file.FullName);
            UnderFont underfont = getUnderFont(fontName);
            if (underfont == null)
                continue;
            dict[fontName.ToLower()] = underfont;
        }*/
    }

    public static UnderFont tryLoad(string key) {
        string k = key;
        key = key.ToLower();
        if (dictMod.ContainsKey(key) || dictDefault.ContainsKey(key)) {
            UnderFont underfont = getUnderFont(k);
            //if (underfont != null)
            dict[key] = underfont;
        } else
            return null;
        return dict[key];
    }

    private static UnderFont getUnderFont(string fontName) {
        XmlDocument xml = new XmlDocument();
        string fontPath = FileLoader.requireFile("Sprites/UI/Fonts/" + fontName + ".png");
        string xmlPath = FileLoader.requireFile("Sprites/UI/Fonts/" + fontName + ".xml", false);
        if (xmlPath == null)
            return null;
        xml.Load(xmlPath);
        Dictionary<char, Sprite> fontMap = loadBuiltinFont(xml["font"]["spritesheet"], fontPath);

        UnderFont underfont = new UnderFont(fontMap);

        if (xml["font"]["voice"] != null)        underfont.Sound = AudioClipRegistry.GetVoice(xml["font"]["voice"].InnerText);
        if (xml["font"]["linespacing"] != null)  underfont.LineSpacing = ParseUtil.getFloat(xml["font"]["linespacing"].InnerText);
        if (xml["font"]["charspacing"] != null)  underfont.CharSpacing = ParseUtil.getFloat(xml["font"]["charspacing"].InnerText);
        if (xml["font"]["color"] != null)        underfont.DefaultColor = ParseUtil.getColor(xml["font"]["color"].InnerText);

        return underfont;
    }

    private static Dictionary<char, Sprite> loadBuiltinFont(XmlNode sheetNode, string fontPath) {
        Sprite[] letterSprites = SpriteUtil.atlasFromXml(sheetNode, SpriteUtil.fromFile(fontPath));
        Dictionary<char, Sprite> letters = new Dictionary<char, Sprite>();
        foreach (Sprite s in letterSprites) {
            string name = s.name;
            if (name.Length == 1) {
                letters.Add(name[0], s);
                continue;
            } else
                switch (name) {
                    case "slash":         letters.Add('/', s);   break;
                    case "dot":           letters.Add('.', s);   break;
                    case "pipe":          letters.Add('|', s);   break;
                    case "backslash":     letters.Add('\\', s);  break;
                    case "colon":         letters.Add(':', s);   break;
                    case "questionmark":  letters.Add('?', s);   break;
                    case "doublequote":   letters.Add('"', s);   break;
                    case "asterisk":      letters.Add('*', s);   break;
                    case "space":         letters.Add(' ', s);   break;
                    case "lt":            letters.Add('<', s);   break;
                    case "rt":            letters.Add('>', s);   break;
                    case "ampersand":     letters.Add('&', s);   break;
                    case "infinity":      letters.Add('∞', s);   break;
                }
        }
        return letters;
    }
}