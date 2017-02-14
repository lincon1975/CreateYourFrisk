﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoonSharp.Interpreter;

/// <summary>
/// Static placeholder inventory class for the player. Will probably get moved to something else that makes sense, like the player...or not.
/// </summary>
public static class Inventory {
    public static string[] addedItems = new string[] { };
    public static int[] addedItemsTypes = new int[] { };
    public static LuaInventory luaInventory;
    public static int tempAmount = 0;
    public static Dictionary<string, string> NametoDesc = new Dictionary<string, string>(), NametoShortName = new Dictionary<string, string>();
    public static Dictionary<string, int> NametoType = new Dictionary<string, int>();
    public static bool usedItemNoDelete = false;
    public static bool overworld = false;
    public static List<UnderItem> container = new List<UnderItem> { new UnderItem("Testing Dog") };

    public static void SetItemList(string[] items = null) {
        container = new List<UnderItem>(new UnderItem[] { });
        if (items != null)
            for (int i = 0; i < items.Length; i++) {
                if (i == 8) {
                    UnitaleUtil.displayLuaError("Setting the inventory", "You added too much items. The inventory can only contain 8 items.");
                    break;
                }
                container.Add(new UnderItem(items[i]));
            }
    }

    public static void SetItem(int index, string Name) {
        if (index > 7)                      UnitaleUtil.displayLuaError("Setting an item", "The inventory can only contain 8 items.");
        else if (index >= container.Count)  AddItem(Name);
        else                                container[index] = new UnderItem(Name);
    }

    public static void RemoveItem(int index) {
        try { container.RemoveAt(index); } 
        catch { } }

    public static bool AddItem(string Name) {
        if (container.Count == 8)
            return false;
        container.Add(new UnderItem(Name));
        return true;
    }

    private static bool CallOnSelf(string func, DynValue[] param = null) {
        bool result;
        if (param != null)
            result = TryCall(func, param);
        else
            result = TryCall(func);
        return result;
    }

    public static bool TryCall(string func, DynValue[] param = null) {
        bool overworld = false;
        if (GameObject.Find("Main Camera OW"))
            overworld = true;
        if (!overworld)
            try {
                if (LuaEnemyEncounter.script.GetVar(func) == null)
                    return false;
                if (param != null)  LuaEnemyEncounter.script.Call(func, param);
                else                LuaEnemyEncounter.script.Call(func);
                return true;
            } catch (InterpreterException ex) {
                UnitaleUtil.displayLuaError(StaticInits.ENCOUNTER, ex.DecoratedMessage);
                return true;
            }
        else
            return false;
    }

    public static void UseItem(int ID) {
        if (SceneManager.GetActiveScene().name == "Battle")  overworld = false;
        else                                                 overworld = true;
        string Name = container[ID].Name, replacement = null; bool inverseRemove = false; int type = container[ID].Type, amount = 0;
        CallOnSelf("HandleItem", new DynValue[] { DynValue.NewString(Name.ToUpper()) });

        TextMessage[] mess = new TextMessage[] { };
        if (addedItems.Length != 0) {
            for (int i = 0; i < addedItems.Length; i++)
                if (addedItems[i].ToLower() == Name.ToLower()) {
                    type = addedItemsTypes[i];
                    if (type == 1 || type == 2)
                        mess = ChangeEquipment(type, Name, mess, out replacement);
                    if (replacement != null) {
                        usedItemNoDelete = false;
                        container.RemoveAt(ID);
                        container.Insert(ID, new UnderItem(replacement));
                    } else if (!usedItemNoDelete && type == 0)
                        container.RemoveAt(ID);
                    else
                        usedItemNoDelete = false;
                    if ((type == 1 || type == 2) && mess.Length != 0 && !UIController.instance.battleDialogued)
                        UIController.instance.ActionDialogResult(mess, UIController.UIState.ENEMYDIALOGUE);
                    return;
                }
        }
        ItemLibrary(Name, type, out mess, out replacement, out amount);
        if (type == 1 || type == 2)
            mess = ChangeEquipment(type, Name, mess, out replacement);
        if (replacement != null) {
            container.RemoveAt(ID);
            container.Insert(ID, new UnderItem(replacement));
        } else if ((!inverseRemove && type == 0) || (inverseRemove && type != 0))
            container.RemoveAt(ID);
        if (!overworld) {
            if (!UIController.instance.battleDialogued && mess.Length != 0)
                UIController.instance.ActionDialogResult(mess, UIController.UIState.ENEMYDIALOGUE);
        } else
            GameObject.Find("TextManager OW").GetComponent<TextManager>().setTextQueue(mess);
       
        return;
    }
    
    public static void AddItemsToDictionaries() {
        NametoDesc.Add("Testing Dog", "A dog that tests something.\rDon't ask me what, I don't know.");        NametoShortName.Add("Testing Dog", "TestDog");          NametoType.Add("Testing Dog", 3);            
        NametoDesc.Add("Bandage", "It has already been used\rseveral times.");                                  
        NametoDesc.Add("Monster Candy", "Has a distinct, non-licorice\rflavor.");                              NametoShortName.Add("Monster Candy", "MnstrCndy");
        NametoDesc.Add("Spider Donut", "A donut made with Spider Cider\rin the batter.");                      NametoShortName.Add("Spider Donut", "SpidrDont");
        NametoDesc.Add("Spider Cider", "Made with whole spiders, not just\rthe juice.");                       NametoShortName.Add("Spider Cider", "SpidrCidr");
        NametoDesc.Add("Butterscotch Pie", "Butterscotch-cinnamon pie,\rone slice.");                          NametoShortName.Add("Butterscotch Pie", "ButtsPie");
        NametoDesc.Add("Snail Pie", "Heals Some HP. An acquired taste.");                                     
        NametoDesc.Add("Snowman Piece", "Please take this to the ends\rof the earth.");                        NametoShortName.Add("Snowman Piece", "SnowPiece");
        NametoDesc.Add("Nice Cream", "Instead of a joke, the wrapper\rsays something nice.");                  NametoShortName.Add("Nice Cream", "NiceCream");
        NametoDesc.Add("Bisicle", "It's a two-pronged popsicle,\rso you can eat it twice.");                    
        NametoDesc.Add("Unisicle", "It's a SINGLE-pronged popsicle.\rWait, that's just normal...");             
        NametoDesc.Add("Cinnamon Bunny", "A cinnamon roll in the shape\rof a bunny.");                         NametoShortName.Add("Cinnamon Bunny", "CinnaBun");
        NametoDesc.Add("Astronaut Food", "For feeding a pet astronaut.");                                      NametoShortName.Add("Astronaut Food", "AstroFood");
        NametoDesc.Add("Crab Apple", "An aquatic fruit that resembles\ra crustacean.");                        NametoShortName.Add("Crab Apple", "CrabApple");
        NametoDesc.Add("Sea Tea", "Made from glowing marsh water.\rIncreases SPEED for one battle.");           
        NametoDesc.Add("Abandoned Quiche", "A psychologically damaged\rspinach egg pie.");                     NametoShortName.Add("Abandoned Quiche", "Ab Quiche");
        NametoDesc.Add("Temmie Flakes", "It's just torn up pieces of\rconstruction paper.");                   NametoShortName.Add("Temmie Flakes", "TemFlakes");
        NametoDesc.Add("Dog Salad", "Recovers HP (Hit Poodles)");                                              
        NametoDesc.Add("Instant Noodles", "Comes with everything you need\rfor a quick meal!");                NametoShortName.Add("Instant Noodles", "InstaNood");
        NametoDesc.Add("Hot Dog...?", "The \"meat\" is made of something\rcalled a \"water sausage.\"");       NametoShortName.Add("Hot Dog...?", "Hot Dog");
        NametoDesc.Add("Hot Cat", "Like a hot dog, but with\rlittle cat ears on the end.");                     
        NametoDesc.Add("Junk Food", "Food that was probably once\rthrown away.");                               
        NametoDesc.Add("Hush Puppy", "This wonderful spell will stop\ra dog from casting magic.");             NametoShortName.Add("Hush Puppy", "HushPupe");
        NametoDesc.Add("Starfait", "A sweet treat made of sparkling stars.");                                  
        NametoDesc.Add("Glamburger", "A hamburger made of edible\rglitter and sequins.");                      NametoShortName.Add("Glamburger", "GlamBurg");
        NametoDesc.Add("Legendary Hero", "Sandwich shaped like a sword.\rIncreases ATTACK when eaten.");       NametoShortName.Add("Legendary Hero", "Leg.Hero");
        NametoDesc.Add("Steak in the Shape of Mettaton's Face", "Huge steak in the shape of\rMettaton's face.You don't feel\rlike it's made of real meat...");
                                                                                                               NametoShortName.Add("Steak in the Shape of Mettaton's Face", "FaceSteak");
        NametoDesc.Add("Popato Chisps", "Regular old popato chisps.");                                         NametoShortName.Add("Popato Chisps", "PT Chisps");
        NametoDesc.Add("Bad Memory", "?????");                                                                 NametoShortName.Add("Bad Memory", "BadMemory");
        NametoDesc.Add("Last Dream", "The goal of \"Determination\".");                                        NametoShortName.Add("Last Dream", "LastDream");

        NametoDesc.Add("Stick", "Its bark is worse than\rits bite. ");                                                                                                 NametoType.Add("Stick", 3);
        NametoDesc.Add("Toy Knife", "Made of plastic. A rarity\rnowadays.");                                                                                           NametoType.Add("Toy Knife", 1);
        NametoDesc.Add("Tough Glove", "A worn pink leather glove.\rFor five-fingered folk.");                  NametoShortName.Add("Tough Glove", "TuffGlove");        NametoType.Add("Tough Glove", 1);
        NametoDesc.Add("Ballet Shoes", "These used shoes make you\rfeel incredibly dangerous.");               NametoShortName.Add("Ballet Shoes", "BallShoes");       NametoType.Add("Ballet Shoes", 1);
        NametoDesc.Add("Torn Notebook", "Contains illegible scrawls.\rIncreases INV by 6.");                   NametoShortName.Add("Torn Notebook", "TornNotbo");      NametoType.Add("Torn Notebook", 1);
        NametoDesc.Add("Burnt Pan", "Damage is rather consistent.\rConsumable items heal four more HP.");                                                              NametoType.Add("Burnt Pan", 1);
        NametoDesc.Add("Empty Gun", "An antique revolver. It has no\rammo. Must be used precisely,\ror damage will be low.");                                          NametoType.Add("Empty Gun", 1);
        NametoDesc.Add("Worn Dagger", "Perfect for cutting plants\rand vines.");                               NametoShortName.Add("Worn Dagger", "WornDG");           NametoType.Add("Worn Dagger", 1);
        NametoDesc.Add("Real Knife", "Here we are!");                                                          NametoShortName.Add("Real Knife", "RealKnife");         NametoType.Add("Real Knife", 1);

        NametoDesc.Add("Faded Ribbon", "If you're cuter, monsters\rwon't hit you as hard.");                   NametoShortName.Add("Faded Ribbon", "Ribbon");          NametoType.Add("Faded Ribbon", 2);
        NametoDesc.Add("Manly Bandanna", "It has seen some wear.\rIt has abs drawn on it.");                   NametoShortName.Add("Manly Bandanna", "Bandanna");      NametoType.Add("Manly Bandanna", 2);
        NametoDesc.Add("Old Tutu", "Finally, a protective piece\rof armor.");                                                                                          NametoType.Add("Old Tutu", 2);
        NametoDesc.Add("Cloudy Glasses", "Glasses marred with wear.\rIncreases INV by 9.");                    NametoShortName.Add("Cloudy Glasses", "ClodGlass");     NametoType.Add("Cloudy Glasses", 2);
        NametoDesc.Add("Temmie Armor", "The things you can do with a\rcollege education! Raises ATTACK when\rworn. Recovers HP every other\rturn. INV up slightly."); 
                                                                                                               NametoShortName.Add("Temmie Armor", "Temmie AR");       NametoType.Add("Temmie Armor", 2);
        NametoDesc.Add("Stained Apron", "Heals 1 HP every other turn.");                                       NametoShortName.Add("Stained Apron", "StainApro");      NametoType.Add("Stained Apron", 2);
        NametoDesc.Add("Cowboy Hat", "This battle-worn hat makes you\rwant to grow a beard. It also\rraises ATTACK by 5.");
                                                                                                               NametoShortName.Add("Cowboy Hat", "CowboyHat");         NametoType.Add("Cowboy Hat", 2);
        NametoDesc.Add("Heart Locket", "It says \"Best Friends Forever.\"");                                   NametoShortName.Add("Heart Locket", "<--Locket");       NametoType.Add("Heart Locket", 2);
        NametoDesc.Add("The Locket", "You can feel it beating.");                                              NametoShortName.Add("The Locket", "TheLocket");         NametoType.Add("The Locket", 2);
    }

    public static void UpdateEquipBonuses() {
        TextMessage[] mess = new TextMessage[] { }; int amount; string replacement;
        ItemLibrary(PlayerCharacter.instance.Weapon, 1, out mess, out replacement, out amount);
        PlayerCharacter.instance.WeaponATK = amount;
        ItemLibrary(PlayerCharacter.instance.Armor, 2, out mess, out replacement, out amount);
        PlayerCharacter.instance.ArmorDEF = amount;
    }

    public static void ItemLibrary(string name, int type, out TextMessage[] mess, out string replacement, out int amount) {
        replacement = null; mess = new TextMessage[] { }; amount = 0;
        switch (type) {
            case 0:
                switch (name) {
                    case "Bandage": mess = new TextMessage[] { new TextMessage("[health:10]You re-applied the bandage.[w:10]\rStill kind of gooey.[w:10]\nYou recovered 10 HP!", true, false) }; break;
                    case "Monster Candy": mess = new TextMessage[] { new TextMessage("[health:10]You ate the Monster Candy.[w:10]\rVery un-licorice-like.[w:10]\nYou recovered 10 HP!", true, false) }; break;
                    case "Spider Donut": mess = new TextMessage[] { new TextMessage("[health:12]Don't worry,[w:5]spider didn't.[w:10]\nYou recovered 12 HP!", true, false) }; break;
                    case "Spider Cider": mess = new TextMessage[] { new TextMessage("[health:24]You drank the Spider Cider.[w:10]\nYou recovered 24 HP!", true, false) }; break;
                    case "Butterscotch Pie": mess = new TextMessage[] { new TextMessage("[health:Max]You ate the Butterscotch Pie.[w:10]\nYour HP was maxed out.", true, false) }; break;
                    case "Snail Pie": mess = new TextMessage[] { new TextMessage("[health:Max - 1]You ate the Snail Pie.[w:10]\nYour HP was maxed out.", true, false) }; break;
                    case "Snowman Piece": mess = new TextMessage[] { new TextMessage("[health:45]You ate the Snowman Piece.[w:10]\nYou recovered 45 HP!", true, false) }; break;
                    case "Nice Cream":
                        int randomCream = Math.randomRange(0, 8); string sentenceCream = "[w:10]\nYou recovered 15 HP!";
                        switch (randomCream) {
                            case 0: sentenceCream = "[health:15]You're super spiffy!" + sentenceCream; break;
                            case 1: sentenceCream = "[health:15]Are those claws natural?" + sentenceCream; break;
                            case 2: sentenceCream = "[health:15]Love yourself! I love you!" + sentenceCream; break;
                            case 3: sentenceCream = "[health:15]You look nice today!" + sentenceCream; break;
                            case 4: sentenceCream = "[health:15](An illustration of a hug)" + sentenceCream; break;
                            case 5: sentenceCream = "[health:15]Have a wonderful day!" + sentenceCream; break;
                            case 6: sentenceCream = "[health:15]Is this as sweet as you?" + sentenceCream; break;
                            case 7: sentenceCream = "[health:15]You're just great!" + sentenceCream; break;
                        }
                        mess = new TextMessage[] { new TextMessage(sentenceCream, true, false) }; break;
                    case "Bisicle":
                        mess = new TextMessage[] { new TextMessage("[health:11]You ate one half of\rthe Bisicle.[w:10]\nYou recovered 11 HP!", true, false) };
                        replacement = "Unisicle"; break;
                    case "Unisicle": mess = new TextMessage[] { new TextMessage("[health:11]You ate the Unisicle.[w:10]\nYou recovered 11 HP!", true, false) }; break;
                    case "Cinnabon Bunny": mess = new TextMessage[] { new TextMessage("[health:22]You ate the Cinnabon Bun.[w:10]\nYou recovered 22 HP!", true, false) }; break;
                    case "Astronaut Food": mess = new TextMessage[] { new TextMessage("[health:21]You ate the Astronaut Food.[w:10]\nYou recovered 21 HP!", true, false) }; break;
                    case "Crab Apple": mess = new TextMessage[] { new TextMessage("[health:18]You ate the Crab Apple.[w:10]\nYou recovered 18 HP!", true, false) }; break;
                    case "Sea Tea":
                        mess = new TextMessage[] { new TextMessage("[health:10][sound:SeaTea]You drank the Sea Tea.[w:10]\nYour SPEED boosts![w:10]\nYou recovered 18 HP!", true, false),
                                                                 new TextMessage("[music:pause][waitall:10]...[waitall:1]but for now stats\rdoesn't change.", true, false),
                                                                 new TextMessage("[noskip][music:unpause][next]", true, false)}; break;
                    case "Abandoned Quiche": mess = new TextMessage[] { new TextMessage("[health:34]You ate the quiche.[w:10]\nYou recovered 34 HP!", true, false) }; break;
                    case "Temmie Flakes": mess = new TextMessage[] { new TextMessage("[health:2]You ate the Temmie Flakes.[w:10]\nYou recovered 2 HP!", true, false) }; break;
                    case "Dog Salad":
                        int randomSalad = Math.randomRange(0, 4); string sentenceSalad;
                        switch (randomSalad) {
                            case 0: sentenceSalad = "[health:2]Oh. These are bones...[w:10]\rYou recovered 2 HP!"; break;
                            case 1: sentenceSalad = "[health:10]Oh. Fries tennis ball...[w:10]\rYou recovered 10 HP!"; break;
                            case 2: sentenceSalad = "[health:30]Oh. Tastes yappy...[w:10]\rYou recovered 30 HP!"; break;
                            default: sentenceSalad = "[health:Max]It's literally garbage???[w:10]\rYour HP was maxed out."; break;
                        }
                        mess = new TextMessage[] { new TextMessage(sentenceSalad, true, false) }; break;
                    case "Instant Noodles":
                        mess = new TextMessage[] { new TextMessage("You remove the Instant\rNoodles from their\rpackaging.", true, false),
                                                   new TextMessage("You put some water in\rthe pot and place it\ron the heat.", true, false),
                                                   new TextMessage("You wait for the water\rto boil...", true, false),
                                                   new TextMessage("[noskip][music:pause]...[w:30]\n...[w:30]\n...", true, false),
                                                   new TextMessage("[noskip]It's[w:30] boiling.", true, false),
                                                   new TextMessage("[noskip]You place the noodles[w:30]\rinto the pot.", true, false),
                                                   new TextMessage("[noskip]4[w:30] minutes left[w:30] until\rthe noodles[w:30] are finished.", true, false),
                                                   new TextMessage("[noskip]3[w:30] minutes left[w:30] until\rthe noodles[w:30] are finished.", true, false),
                                                   new TextMessage("[noskip]2[w:30] minutes left[w:30] until\rthe noodles[w:30] are finished.", true, false),
                                                   new TextMessage("[noskip]1[w:30] minute left[w:30] until\rthe noodles[w:30] are finished.", true, false),
                                                   new TextMessage("[noskip]The noodles[w:30] are finished.", true, false),
                                                   new TextMessage("...they don't taste very\rgood.", true, false),
                                                   new TextMessage("You add the flavor packet.", true, false),
                                                   new TextMessage("That's better.", true, false),
                                                   new TextMessage("Not great,[w:5] but better.", true, false),
                                                   new TextMessage("[music:unpause][health:4]You ate the Instant Noodles.[w:10]\nYou recovered 4 HP!", true, false)}; break;
                    case "Hot Dog...?": mess = new TextMessage[] { new TextMessage("[health:20][sound:HotDog]You ate the Hot Dog.[w:10]\nYou recovered 20 HP!", true, false) }; break;
                    case "Hot Cat": mess = new TextMessage[] { new TextMessage("[health:21][sound:HotCat]You ate the Hot Cat.[w:10]\nYou recovered 21 HP!", true, false) }; break;
                    case "Junk Food": mess = new TextMessage[] { new TextMessage("[health:17]You ate the Junk Food.[w:10]\nYou recovered 17 HP!", true, false) }; break;
                    case "Hush Puppy": mess = new TextMessage[] { new TextMessage("[health:65]You ate the Hush Puppy.[w:10]\rDog-magic is neutralized.[w:10]\nYou recovered 65 HP!", true, false) }; break;
                    case "Starfait": mess = new TextMessage[] { new TextMessage("[health:14]You ate the Starfait.[w:10]\nYou recovered 14 HP!", true, false) }; break;
                    case "Glamburger": mess = new TextMessage[] { new TextMessage("[health:27]You ate the Glamburger.[w:10]\nYou recovered 27 HP!", true, false) }; break;
                    case "Legendary Hero":
                        mess = new TextMessage[] { new TextMessage("[health:40][sound:LegHero]You ate the Legendary Hero.[w:10]\nATTACK increased by 4 ![w:10]\nYou recovered 40 HP!", true, false),
                                                                 new TextMessage("[music:pause][waitall:10]...[waitall:1]but for now stats\rdoesn't change.", true, false),
                                                                 new TextMessage("[noskip][music:unpause][next]", true, false)}; break;
                    case "Steak in the Shape of Mettaton's Face": mess = new TextMessage[] { new TextMessage("[health:60]You ate the Face Steak.[w:10]\nYou recovered 60 HP!", true, false) }; break;
                    case "Popato Chisps": mess = new TextMessage[] { new TextMessage("[health:13]You ate the Popato Chisps.[w:10]\nYou recovered 13 HP!", true, false) }; break;
                    case "Bad Memory":
                        if (PlayerCharacter.instance.HP <= 3) mess = new TextMessage[] { new TextMessage("[health:Max]You consume the Bad Memory.[w:10]\nYour HP was maxed out.", true, false) };
                        else mess = new TextMessage[] { new TextMessage("[health:-1]You consume the Bad Memory.[w:10]\nYou lost 1 HP.", true, false) };
                        break;
                    case "Last Dream": mess = new TextMessage[] { new TextMessage("[health:17]Through DETERMINATION,\rthe dream became true.[w:10]\nYou recovered 17 HP!", true, false) }; break;
                    default: UnitaleUtil.writeInLog("The item doesn't exists in this pool."); break;
                }
                break;
            case 1:
                switch (name) {
                    case "Toy Knife": amount = 3; break;
                    case "Tough Glove": amount = 5; break;
                    case "Ballet Shoes": amount = 7; break;
                    case "Torn Notebook": amount = 2; break;
                    case "Burnt Pan": amount = 10; break;
                    case "Empty Gun": amount = 12; break;
                    case "Worn Dagger": amount = 15; break;
                    case "Real Knife": amount = 99; break;
                    default: UnitaleUtil.writeInLog("The item doesn't exists in this pool."); break;
                }
                break;
            case 2:
                switch (name) {
                    case "Faded Ribbon": amount = 3; break;
                    case "Manly Bandanna": amount = 7; break;
                    case "Old Tutu": amount = 10; break;
                    case "Cloudy Glasses": amount = 6; break;
                    case "Stained Apron": amount = 11; break;
                    case "Cowboy Hat": amount = 12; break;
                    case "Heart Locket": amount = 15; break;
                    case "The Locket": amount = 99; break;
                    default: UnitaleUtil.writeInLog("The item doesn't exists in this pool."); break;
                }
                break;
            default:
                switch (name) {
                    case "Testing Dog": mess = new TextMessage[] { new TextMessage("This dog is testing something.", true, false), new TextMessage("I must leave it alone.", true, false) }; break;
                    case "Stick": mess = new TextMessage[] { new TextMessage("You throw the stick.[w:10]\nNothing happens.", true, false) }; break;
                    default: UnitaleUtil.writeInLog("The item doesn't exists in this pool."); break;
                }
                break;
        }
    }

    public static TextMessage[] ChangeEquipment(int mode, string Name, TextMessage[] mess, out string replacement) {
        replacement = "";
        if (mode == 1) {
            PlayerCharacter.instance.WeaponATK = tempAmount;
            replacement = PlayerCharacter.instance.Weapon;
            PlayerCharacter.instance.Weapon = Name;
        } else if (mode == 2) {
            PlayerCharacter.instance.ArmorDEF = tempAmount;
            replacement = PlayerCharacter.instance.Armor;
            PlayerCharacter.instance.Armor = Name;
        } else
            UnitaleUtil.displayLuaError("Equip an item", "The item \"" + Name + "\" can't be equipped.");
        if (mess.Length == 0) mess = new TextMessage[] { new TextMessage("You equipped " + Name + ".", true, false) };
        else                  mess = new TextMessage[] { };
        return mess;
    }

    public static void RemoveAddedItems() {
        for (int i = 0; i < container.Count; i ++) {
            foreach (string str in addedItems) {
                if (container[i].Name == str) {
                    //UnitaleUtil.writeInLog("The item \"" + str + "\" has been removed from the Inventory.");
                    container.RemoveAt(i);
                    i --;
                    break;
                }
            }
        }
        foreach (string str in addedItems) {

            if (str == PlayerCharacter.instance.Weapon && PlayerCharacter.instance.Weapon != "Stick" && !NametoDesc.ContainsValue(str)) {
                for (int i = 0; i < container.Count; i++)
                    if (container[i].Name == "Stick") {
                        container.RemoveAt(i);
                        break;
                    }
                PlayerCharacter.instance.Weapon = "Stick";
                PlayerCharacter.instance.WeaponATK = 0;
                //UnitaleUtil.writeInLog("The item \"" + str + "\" has been removed from the weapon Equipment.");
            } else if (str == PlayerCharacter.instance.Weapon && PlayerCharacter.instance.Weapon != "Stick" && NametoDesc.ContainsValue(str)) {
                TextMessage[] mess; string replacement; int amount;
                ItemLibrary(str, 1, out mess, out replacement, out amount);
                PlayerCharacter.instance.WeaponATK = amount;
                //UnitaleUtil.writeInLog("The item \"" + str + "\" has been setted to the right value for Weapon.");
            }

            if (str == PlayerCharacter.instance.Armor && PlayerCharacter.instance.Armor != "Bandage" && !NametoDesc.ContainsValue(str)) {
                for (int i = 0; i < container.Count; i++)
                    if (container[i].Name == "Bandage") {
                        container.RemoveAt(i);
                        break;
                    }
                PlayerCharacter.instance.Armor = "Bandage";
                //UnitaleUtil.writeInLog("The item \"" + str + "\" has been removed from the armor Equipment.");
            } else if (str == PlayerCharacter.instance.Armor && PlayerCharacter.instance.Armor != "Bandage" && NametoDesc.ContainsValue(str)) {
                TextMessage[] mess; string replacement; int amount;
                ItemLibrary(str, 2, out mess, out replacement, out amount);
                PlayerCharacter.instance.ArmorDEF = amount;
                //UnitaleUtil.writeInLog("The item \"" + str + "\" has been setted to the right value for Armor.");
            }
        }
        addedItems = new string[] { }; addedItemsTypes = new int[] { };
    }
}