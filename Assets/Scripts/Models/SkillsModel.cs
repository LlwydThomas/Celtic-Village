using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillModel {
    public List<Pawn> pawnList = new List<Pawn>();
    public List<SkillData> skillDataList = new List<SkillData>();
    public List<int> pawnIDs = new List<int>();
    public Dictionary<int, Pawn> pawnLookup = new Dictionary<int, Pawn>();
    public Dictionary<int, SkillData> skillDataLookup = new Dictionary<int, SkillData>();
    public Dictionary<GameObject, Pawn> pawnObjectConnect = new Dictionary<GameObject, Pawn>();
}