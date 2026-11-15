using System.Collections.Generic;
using UnityEngine;

public class NfcHandler : MonoBehaviour
{
    public List<Wrapper> nfcs = new List<Wrapper>();
    [Header("---Default---")]
    [SerializeField] private GameObject defaultState;
    [Header("---Test---")]
    [SerializeField] private int _numberTest;

    private void Awake() => SetDefaultState();
    private void OnEnable() => Subscribe();
    private void OnDisable() => Unsubscribe();

    public void Subscribe()
    {
        NfcService.OnTagRead -= HandleTagRead;
        NfcService.OnTagRead += HandleTagRead;
    }
    public void Unsubscribe()
    {
        NfcService.OnTagRead -= HandleTagRead;
    }
    private void HandleTagRead(NfcModel model) => SetState(model.Contenido);

    public void SetState(string contenido)
    {
        nfcs.ForEach(m => m.ObjetEnable.SetActive(false));

        var element = nfcs.Find(m => m.Id.ToString().Equals(contenido));
        if (element != null)
        {
            SetDefaultStateActive(false);
            element.ObjetEnable.SetActive(true);
            return;
        }

        SetDefaultStateActive(true);
    }

    public void SetState(int id) => SetState(id.ToString());

    public void SetDefaultState()
    {
        nfcs.ForEach(m => m.ObjetEnable.SetActive(false));
        SetDefaultStateActive(true);
    }

    private void SetDefaultStateActive(bool active)
    {
        if (defaultState != null)
            defaultState.SetActive(active);
    }

    [ContextMenu("ActiveTest")]
    private void ActiveTest() 
    {
        HandleTagRead(new NfcModel { Contenido = $"{_numberTest}", Uid = "sasas" });
    }


    [System.Serializable]
    public class Wrapper 
    {
        [SerializeField] private int id;
        [SerializeField] private GameObject objetEnable;
        public GameObject ObjetEnable => objetEnable;
        public int Id => id;
    }
}
