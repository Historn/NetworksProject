using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperStrike
{
    [CreateAssetMenu(fileName = "New Character", menuName = "HyperStrike/Character Data")]
    public class CharacteData : ScriptableObject
    {
        public string characterName;
        public int health;
        public float speed;
        public float basicDamage;

    }
}

