using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace test
{
    public class TestClassA : CharacterRegistry
    {
        private CharacterMovement1 _characterMovement;

        public override void Spawned()
        {
            if (!Object.HasStateAuthority) return;
            InitScript(this);
            Debug.Log("A");
        }

        private void Start()
        {
            Debug.Log("B");
            _characterMovement = GetScript<CharacterMovement1>();
            Debug.Log("Value: " + _characterMovement.CurrentMoveSpeed);
        }

        public void TestZ()
        {
            Debug.Log("TestClassA Reached and used!");
        }
    }
}

