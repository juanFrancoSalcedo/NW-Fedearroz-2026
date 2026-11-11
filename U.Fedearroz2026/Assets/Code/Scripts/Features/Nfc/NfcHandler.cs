using System.Collections.Generic;
using UnityEngine;

public class NfcHandler : MonoBehaviour
{
    public List<Wrapper> nfcs = new List<Wrapper>();
    [Header("---Test---")]
    [SerializeField] private int _numberTest;

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
    private void HandleTagRead(NfcModel model)
    {
        nfcs.ForEach(m=>m.ObjetEnable.SetActive(false));
        var element = nfcs.Find(m => m.Id.ToString().Equals(model.Contenido));
        if (element != null)
            element.ObjetEnable.SetActive(true);
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
