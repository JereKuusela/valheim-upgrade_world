using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace UpgradeWorld;

[Serializable]
public class Vegetation {
  public string prefab = "";
  public float min = 1f;
  public float max = 1f;
  public bool forcePlacement = false;
  public float scaleMin = 1f;
  public float scaleMax = 1f;
  public float randTilt = 0f;
  public float chanceToUseGroundTilt = 0f;
  public string[] biome = new string[0];
  public string[] biomeArea = new string[0];
  public bool blockCheck = true;
  public float minAltitude = 0f;
  public float maxAltitude = 1000f;
  public float minOceanDepth = 0f;
  public float maxOceanDepth = 0f;
  public float minTilt = 0f;
  public float maxTilt = 90f;
  public float terrainDeltaRadius = 0f;
  public float maxTerrainDelta = 10f;
  public float minTerrainDelta = 0f;
  public bool snapToWater = false;
  public float groundOffset = 0f;
  public int groupSizeMin = 1;
  public int groupSizeMax = 1;
  public float groupRadius = 0f;
  public bool inForest = false;
  public float forestTresholdMin = 0f;
  public float forestTresholdMax = 0f;
}

public class VegetationData {
  private static string[] FromBiomes(Heightmap.Biome biome) {
    List<string> biomes = new();
    if ((biome & Heightmap.Biome.AshLands) > 0) {
      biomes.Add("ashlands");
      biome -= Heightmap.Biome.AshLands;
    }
    if ((biome & Heightmap.Biome.BlackForest) > 0) {
      biomes.Add("blackforest");
      biome -= Heightmap.Biome.BlackForest;
    }
    if ((biome & Heightmap.Biome.DeepNorth) > 0) {
      biomes.Add("deepnorth");
      biome -= Heightmap.Biome.DeepNorth;
    }
    if ((biome & Heightmap.Biome.Meadows) > 0) {
      biomes.Add("meadows");
      biome -= Heightmap.Biome.Meadows;
    }
    if ((biome & Heightmap.Biome.Mistlands) > 0) {
      biomes.Add("mistlands");
      biome -= Heightmap.Biome.Mistlands;
    }
    if ((biome & Heightmap.Biome.Mountain) > 0) {
      biomes.Add("mountain");
      biome -= Heightmap.Biome.Mountain;
    }
    if ((biome & Heightmap.Biome.Ocean) > 0) {
      biomes.Add("ocean");
      biome -= Heightmap.Biome.Ocean;
    }
    if ((biome & Heightmap.Biome.Plains) > 0) {
      biomes.Add("plains");
      biome -= Heightmap.Biome.Plains;
    }
    if ((biome & Heightmap.Biome.Swamp) > 0) {
      biomes.Add("swamp");
      biome -= Heightmap.Biome.Swamp;
    }
    if (biome > 0) biomes.Add(biome.ToString());
    return biomes.ToArray();
  }
  private static string[] FromBiomeAreas(Heightmap.BiomeArea biomeArea) {
    List<string> biomesAreas = new();
    if ((biomeArea & Heightmap.BiomeArea.Edge) > 0) {
      biomesAreas.Add("edge");
      biomeArea -= Heightmap.BiomeArea.Edge;
    }
    if ((biomeArea & Heightmap.BiomeArea.Median) > 0) {
      biomesAreas.Add("median");
      biomeArea -= Heightmap.BiomeArea.Median;
    }
    if (biomeArea > 0) biomesAreas.Add(biomeArea.ToString());
    return biomesAreas.ToArray();
  }
  private static Heightmap.Biome ToBiomes(string[] m_biome) {
    Heightmap.Biome result = 0;
    foreach (var biome in m_biome) {
      var name = biome.ToLower();
      if (name == "ashlands") result += (int)Heightmap.Biome.AshLands;
      else if (name == "blackforest") result += (int)Heightmap.Biome.BlackForest;
      else if (name == "deepnorth") result += (int)Heightmap.Biome.DeepNorth;
      else if (name == "meadows") result += (int)Heightmap.Biome.Meadows;
      else if (name == "mistlands") result += (int)Heightmap.Biome.Mistlands;
      else if (name == "mountain") result += (int)Heightmap.Biome.Mountain;
      else if (name == "ocean") result += (int)Heightmap.Biome.Ocean;
      else if (name == "plains") result += (int)Heightmap.Biome.Plains;
      else if (name == "swamp") result += (int)Heightmap.Biome.Swamp;
      else {
        if (int.TryParse(biome, out var value)) result += value;
        else throw new InvalidOperationException($"Invalid biome {biome}.");
      }
    }
    return result;
  }
  private static Heightmap.BiomeArea ToBiomeAreas(string[] m_biome) {
    Heightmap.BiomeArea result = 0;
    foreach (var biome in m_biome) {
      var name = biome.ToLower();
      if (name == "edge") result += (int)Heightmap.BiomeArea.Edge;
      else if (name == "median") result += (int)Heightmap.BiomeArea.Median;
      else {
        if (int.TryParse(biome, out var value)) result += value;
        else throw new InvalidOperationException($"Invalid biome area {biome}.");
      }
    }
    return result;
  }
  public static ZoneSystem.ZoneVegetation Deserialize(Vegetation data) {
    var veg = new ZoneSystem.ZoneVegetation();
    if (ZNetScene.instance.m_namedPrefabs.TryGetValue(data.prefab.GetStableHashCode(), out var obj))
      veg.m_prefab = obj;
    else
      throw new InvalidOperationException($"Prefab {data.prefab} not found.");
    veg.m_min = data.min;
    veg.m_max = data.max;
    veg.m_forcePlacement = data.forcePlacement;
    veg.m_scaleMin = data.scaleMin;
    veg.m_scaleMax = data.scaleMax;
    veg.m_randTilt = data.randTilt;
    veg.m_chanceToUseGroundTilt = data.chanceToUseGroundTilt;
    veg.m_biome = ToBiomes(data.biome);
    veg.m_biomeArea = ToBiomeAreas(data.biomeArea);
    veg.m_blockCheck = data.blockCheck;
    veg.m_minAltitude = data.minAltitude;
    veg.m_maxAltitude = data.maxAltitude;
    veg.m_minOceanDepth = data.minOceanDepth;
    veg.m_maxOceanDepth = data.maxOceanDepth;
    veg.m_minTilt = data.minTilt;
    veg.m_maxTilt = data.maxTilt;
    veg.m_terrainDeltaRadius = data.terrainDeltaRadius;
    veg.m_maxTerrainDelta = data.maxTerrainDelta;
    veg.m_minTerrainDelta = data.minTerrainDelta;
    veg.m_snapToWater = data.snapToWater;
    veg.m_groundOffset = data.groundOffset;
    veg.m_groupSizeMin = data.groupSizeMin;
    veg.m_groupSizeMax = data.groupSizeMax;
    veg.m_groupRadius = data.groupRadius;
    veg.m_inForest = data.inForest;
    veg.m_forestTresholdMin = data.forestTresholdMin;
    veg.m_forestTresholdMax = data.forestTresholdMax;
    return veg;
  }
  public static bool IsValid(ZoneSystem.ZoneVegetation veg) => veg.m_prefab && veg.m_prefab.GetComponent<ZNetView>() != null;
  public static Vegetation Serialize(ZoneSystem.ZoneVegetation veg) {
    Vegetation data = new();
    data.prefab = veg.m_prefab.name;
    data.min = veg.m_min;
    data.max = veg.m_max;
    data.forcePlacement = veg.m_forcePlacement;
    data.scaleMin = veg.m_scaleMin;
    data.scaleMax = veg.m_scaleMax;
    data.randTilt = veg.m_randTilt;
    data.chanceToUseGroundTilt = veg.m_chanceToUseGroundTilt;
    data.biome = FromBiomes(veg.m_biome);
    data.biomeArea = FromBiomeAreas(veg.m_biomeArea);
    data.blockCheck = veg.m_blockCheck;
    data.minAltitude = veg.m_minAltitude;
    data.maxAltitude = veg.m_maxAltitude;
    data.minOceanDepth = veg.m_minOceanDepth;
    data.maxOceanDepth = veg.m_maxOceanDepth;
    data.minTilt = veg.m_minTilt;
    data.maxTilt = veg.m_maxTilt;
    data.terrainDeltaRadius = veg.m_terrainDeltaRadius;
    data.maxTerrainDelta = veg.m_maxTerrainDelta;
    data.minTerrainDelta = veg.m_minTerrainDelta;
    data.snapToWater = veg.m_snapToWater;
    data.groundOffset = veg.m_groundOffset;
    data.groupSizeMin = veg.m_groupSizeMin;
    data.groupSizeMax = veg.m_groupSizeMax;
    data.groupRadius = veg.m_groupRadius;
    data.inForest = veg.m_inForest;
    data.forestTresholdMin = veg.m_forestTresholdMin;
    data.forestTresholdMax = veg.m_forestTresholdMax;
    return data;
  }

  public static bool Save() {
    var zs = ZoneSystem.instance;
    if (!zs) return false;
    var jsons = zs.m_vegetation.Where(VegetationData.IsValid).Select(VegetationData.Serialize).Select(data => JsonUtility.ToJson(data, true));
    var content = string.Join(",\n", jsons);
    var saveFile = Application.persistentDataPath + "/vegetation.json";
    File.WriteAllText(saveFile, content);
    return true;
  }
  public static bool Load() {
    var saveFile = Application.persistentDataPath + "/vegetation.json";
    if (!File.Exists(saveFile)) return false;
    try {
      var file = File.ReadAllText(saveFile);
      var data = file.Replace("},", "}|").Split('|').Select(JsonUtility.FromJson<Vegetation>);
      var vegs = data.Select(VegetationData.Deserialize).ToList();
      ZoneSystem.instance.m_vegetation = vegs;

    } catch (Exception e) {
      throw new InvalidOperationException(e.Message);
    }
    return true;
  }
}