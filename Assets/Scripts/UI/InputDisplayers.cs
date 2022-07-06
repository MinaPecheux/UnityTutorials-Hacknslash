using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class InputDisplayers : MonoBehaviour
    {
        public static InputDisplayers instance;

        private List<InputDisplayer> _displayers;

        private void Awake()
        {
            if (instance == null)
                instance = this;

            _displayers = new List<InputDisplayer>();
            foreach (Transform child in transform)
                _displayers.Add(child.GetComponent<InputDisplayer>());
        }

        public void SetDisplays((string, string)[] displays)
        {
            if (displays == null)
                gameObject.SetActive(false);
            else
            {
                for (int i = 0; i < _displayers.Count; i++)
                {
                    if (i < displays.Length)
                    {
                        (string text, string actionPath) = displays[i];
                        _displayers[i].SetDisplay(text, actionPath);
                        _displayers[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        _displayers[i].gameObject.SetActive(false);
                    }
                }
                gameObject.SetActive(true);
            }
        }

        public void SetDisplay(
            int index, string text, string actionPath)
        {
            _displayers[index].SetDisplay(text, actionPath);
        }
    }

}
