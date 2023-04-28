using UnityEngine;

[System.Serializable]
public class PlayerData{

    private float maxVida = 100;
    private float vida;
    private int gold;
    private float ataque;
    private string sceneName;
    //private Transform positionRevivir;
    private float x, y, z;
    private float cargaHabilidadCondor, cargaHabilidadSerpiente, cargaHabilidadLanza;
    public PlayerData(Hoyustus hoyustus) {
        //vida =  hoyustus.getVida();
        ataque = hoyustus.getAtaque();
        gold = hoyustus.getGold();
    }

    public PlayerData(float vida, int gold, string sceneName, float x, float y, float z) { 
        //this.vida = vida;
        this.gold = gold;
        this.sceneName = sceneName;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public float getVida(){
        return vida;
    }

    public int getGold(){
        return gold;
    }

    public float getAtaque(){
        return ataque;
    }

    public string getSceneName(){
        return sceneName;
    }

    public float getX(){
        return x;
    }

    public float getY(){
        return y;
    }

    public float getZ(){
        return z;
    }

}
