using NUnit.Framework;
using UnityEngine;

public class ManagerPeleaMapinguari : MonoBehaviour
{
    [SerializeField] public PlataformaMapinguari[] plataformas = new PlataformaMapinguari[4];
    [SerializeField] private NubeToxica[] nubesToxicas = new NubeToxica[4];
    [SerializeField] private GameObject bossLifeBar;


    public void EliminarLogicaPlataformas() { 
        foreach(PlataformaMapinguari p in plataformas){ 
            Destroy(p);
        }

        foreach (NubeToxica n in nubesToxicas)
        {
            n.IsMapinguariDerrotado(true);
        }
        Destroy(bossLifeBar);
    }
}
