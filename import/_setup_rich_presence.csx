#r "../lib/UndertaleModLib.dll"
#r "../lib/Underanalyzer.dll"

using UndertaleModLib.Util;
using UndertaleModLib.Models;
using UndertaleModLib;
using UndertaleModLib.Decompiler;
using Underanalyzer.Decompiler;


void SetupRichPresence(UndertaleData gameData) {
    var extDllName = "NekoPresence.x64.dll";

    var npExtension = gameData.Extensions.ByName("Steamworks");

    var npFile = new UndertaleExtensionFile() {
        Filename = gameData.Strings.MakeString(extDllName)
    };
    npExtension.Files.Add(npFile);

    uint lastExtFuncId = 0;

    foreach(var file in npExtension.Files) {
        foreach(var func in file.Functions) {
            if(func.ID > lastExtFuncId) {
                lastExtFuncId = func.ID;
            }
        }
    }

    _Patch_NekoPresence_DefineFunction(gameData, npFile, lastExtFuncId);

    var obj = DefineGameObject(gameData, "obj_richpresence");
    obj.Visible = true;
    obj.Persistent = true;
    DefineRoomGameObject(gameData, 0, obj);
}

UndertaleGameObject DefineGameObject(UndertaleData utdata, string name, bool throwIfExists = false) {
    var objDef = utdata.GameObjects.ByName(name);

    if(objDef == null) {
        objDef = new UndertaleGameObject() {
            Name = utdata.Strings.MakeString(name)
        };
        utdata.GameObjects.Add(objDef);
    } else if(throwIfExists) {
        throw new Exception($"Game object with name '{name}' already exists");
    }

    return objDef;
}
UndertaleRoom.GameObject DefineRoomGameObject(UndertaleData utdata, int roomOrderIndex, UndertaleGameObject objDef, bool throwIfExists = false) {
    return DefineRoomGameObject(utdata, utdata.GeneralInfo.RoomOrder[roomOrderIndex].Resource, objDef, throwIfExists);
}

UndertaleRoom.GameObject DefineRoomGameObject(UndertaleData utdata, UndertaleRoom room, UndertaleGameObject objDef, bool throwIfExists = false) {
    UndertaleRoom.GameObject obj = null;

    foreach(var o in room.GameObjects) {
        if(o.ObjectDefinition == objDef) {
            obj = o;
            break;
        }
    }

    if(obj == null) {
        obj = new UndertaleRoom.GameObject() {
            InstanceID = utdata.GeneralInfo.LastObj++,
            ObjectDefinition = objDef,
        };
        room.GameObjects.Add(obj);
    } else if(throwIfExists) {
        throw new Exception($"Game object for '{objDef.Name?.Content}' already exists in room '{room.Name?.Content}' with id '{obj.InstanceID}'");
    }

    return obj;
}


void _Patch_NekoPresence_DefineFunction(UndertaleData utdata, UndertaleExtensionFile file, uint funcIdOffset = 0) {
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "__np_initdll", UndertaleExtensionVarType.Double, "np_initdll");
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "__np_shutdown", UndertaleExtensionVarType.Double, "np_shutdown");
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "np_initdiscord", UndertaleExtensionVarType.Double, "np_initdiscord", UndertaleExtensionVarType.String, UndertaleExtensionVarType.Double, UndertaleExtensionVarType.String);
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "np_setpresence", UndertaleExtensionVarType.Double, "np_setpresence", UndertaleExtensionVarType.String, UndertaleExtensionVarType.String, UndertaleExtensionVarType.String, UndertaleExtensionVarType.String);
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "np_update", UndertaleExtensionVarType.Double, "np_update");
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "__np_registercallbacks_do_not_call", UndertaleExtensionVarType.Double, "RegisterCallbacks", UndertaleExtensionVarType.String, UndertaleExtensionVarType.String, UndertaleExtensionVarType.String, UndertaleExtensionVarType.String);
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "np_setpresence_more", UndertaleExtensionVarType.Double, "np_setpresence_more", UndertaleExtensionVarType.String, UndertaleExtensionVarType.String, UndertaleExtensionVarType.Double);
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "np_clearpresence", UndertaleExtensionVarType.Double, "np_clearpresence");
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "np_registergame", UndertaleExtensionVarType.Double, "np_registergame", UndertaleExtensionVarType.String, UndertaleExtensionVarType.String);
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "np_registergame_steam", UndertaleExtensionVarType.Double, "np_registergame_steam", UndertaleExtensionVarType.String, UndertaleExtensionVarType.String);
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "np_setpresence_secrets", UndertaleExtensionVarType.Double, "np_setpresence_secrets", UndertaleExtensionVarType.String, UndertaleExtensionVarType.String, UndertaleExtensionVarType.String);
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "np_setpresence_partyparams", UndertaleExtensionVarType.Double, "np_setpresence_partyparams", UndertaleExtensionVarType.Double, UndertaleExtensionVarType.Double, UndertaleExtensionVarType.String, UndertaleExtensionVarType.Double);
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "np_respond", UndertaleExtensionVarType.Double, "np_respond", UndertaleExtensionVarType.String, UndertaleExtensionVarType.Double);
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "np_setpresence_timestamps", UndertaleExtensionVarType.Double, "np_setpresence_timestamps", UndertaleExtensionVarType.Double, UndertaleExtensionVarType.Double, UndertaleExtensionVarType.Double);
    file.Functions.DefineExtensionFunction(utdata.Functions, utdata.Strings, ++funcIdOffset, 1, "np_setpresence_buttons", UndertaleExtensionVarType.Double, "np_setpresence_buttons", UndertaleExtensionVarType.Double, UndertaleExtensionVarType.String, UndertaleExtensionVarType.String);

    file.CleanupScript = utdata.Strings.MakeString("__np_shutdown");
    file.InitScript = utdata.Strings.MakeString("__np_initdll");
}
