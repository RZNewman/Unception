using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;

public class AttackBlock : ScriptableObject
{
    public AttackGenerationData source;
    public float powerAtGeneration;
    public bool scales;
    public string id;
    public AttackFlair flair;


}
