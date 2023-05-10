using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tile
{
    //public Hashtable connection = new Hashtable();
    public TileData tileData;
    [HideInInspector]
    public List<Tile> frontNeighbors;
    [HideInInspector]
    public List<Tile> rightNeighbors;
    [HideInInspector]
    public List<Tile> backNeighbors;
    [HideInInspector]
    public List<Tile> leftNeighbors;

    // public void initialize()
    // {
    //     this.connection.Add("front", front) ;
    //     this.connection.Add("right", right) ;
    //     this.connection.Add("back", back) ;
    //     this.connection.Add("left", left) ;
    // }
    public Tile(TileData tileData){
        this.tileData = tileData;
        frontNeighbors = new List<Tile>();
        rightNeighbors = new List<Tile>();
        backNeighbors = new List<Tile>();
        leftNeighbors = new List<Tile>();
    }


    public void analyze(List<Tile> tiles){
        // this.backNeighbors.Clear();
        // this.frontNeighbors.Clear();
        // this.leftNeighbors.Clear();
        // this.rightNeighbors.Clear();
        for(int i=0; i< tiles.Count; i++){
            Tile tile = tiles[i];
            if(tile.tileData.front == this.tileData.back){
                this.backNeighbors.Add(tile);
            }
            if(tile.tileData.right == this.tileData.left){
                this.leftNeighbors.Add(tile);
            }
            if(tile.tileData.left == this.tileData.right){
                this.rightNeighbors.Add(tile);
            }
            if(tile.tileData.back == this.tileData.front){
                this.frontNeighbors.Add(tile);
            }
        }
    }
    public void rotateY90(){
        //this.tileData.tileObject.transform.Rotate(new Vector3(0, 90, 0));
        tileData.rotationY += 90;
        int current = tileData.front;
        tileData.front = tileData.right;
        tileData.right = tileData.back;
        tileData.back = tileData.left;
        tileData.left = current;
    }

    // public void rotateY180(){
    //     transform.Rotate(new Vector3(0, 180, 0));
    //     int current = front;
    //     int current1 = right;
    //     front = back;
    //     right = left;
    //     back = current;
    //     left = current1;
    // }

    // public void rotateY270(){
    //     transform.Rotate(new Vector3(0, 270, 0));
    //     int current = front;
    //     front = left;
    //     left = back;
    //     back = right;
    //     right = current;
    // }
    // public override string ToString() {    
    //     return $"Tile: {{TileData: {tileData.ToString()}, FrontNeighbors: {string.Join(", ", frontNeighbors)}, RightNeighbors: {string.Join(", ", rightNeighbors)}, BackNeighbors: {string.Join(", ", backNeighbors)}, LeftNeighbors: {string.Join(", ", leftNeighbors)}}}";}

}