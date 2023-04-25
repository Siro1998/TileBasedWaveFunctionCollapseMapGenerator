using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile3D
{
    public TileData3D tileData;
    [HideInInspector]
    public List<Tile3D> frontNeighbors;
    [HideInInspector]
    public List<Tile3D> rightNeighbors;
    [HideInInspector]
    public List<Tile3D> backNeighbors;
    [HideInInspector]
    public List<Tile3D> leftNeighbors;
    public List<Tile3D> topNeighbors;
    public List<Tile3D> bottomNeighbors;

    public Tile3D(TileData3D tileData){
        this.tileData = tileData;
        frontNeighbors = new List<Tile3D>();
        rightNeighbors = new List<Tile3D>();
        backNeighbors = new List<Tile3D>();
        leftNeighbors = new List<Tile3D>();
        topNeighbors = new List<Tile3D>();
        bottomNeighbors = new List<Tile3D>();
    }


    public void analyze(List<Tile3D> tiles){
        for(int i=0; i< tiles.Count; i++){
            Tile3D tile = tiles[i];
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
            if(tile.tileData.bottom == this.tileData.top){
                this.topNeighbors.Add(tile);
            }
            if(tile.tileData.top == this.tileData.bottom){
                this.bottomNeighbors.Add(tile);
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