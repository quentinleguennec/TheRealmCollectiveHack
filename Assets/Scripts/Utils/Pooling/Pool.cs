using System.Collections.Generic;
using UnityEngine;

namespace Tengio
{
    public class Pool : MonoBehaviour
    {

        [SerializeField]
        private GameObject poolElementPrefab;
        [SerializeField]
        private int baseSize = 10;

        private List<PoolElement> pool;


        private void Awake()
        {
            InstantiatePool();
        }

        private PoolElement CreateNewElement()
        {
            GameObject elementGameObject = Instantiate(poolElementPrefab, transform);
            PoolElement element = elementGameObject.GetComponent<PoolElement>();
            element.Initialize();
            element.Deactivate();
            return element;
        }

        private void InstantiatePool()
        {
            pool = new List<PoolElement>(baseSize);
            for (int i = 0; i < baseSize; i++)
            {
                pool.Add(CreateNewElement());
            }
        }

        public PoolElement GetElement()
        {
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].IsAvailable())
                {
                    return pool[i];
                }
            }

            PoolElement newElement = CreateNewElement();
            pool.Add(newElement);
            return newElement;
        }
    }
}