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
        tileData.rotationY += 90;
        int current = tileData.front;
        tileData.front = tileData.right;
        tileData.right = tileData.back;
        tileData.back = tileData.left;
        tileData.left = current;
    }
    public void rotateX90(){
        if(tileData.rotationY==0){
            tileData.rotationX += 90;
        }
        else if(tileData.rotationY==90){
            tileData.rotationZ += 90;
        }
        else if(tileData.rotationY==180){
            tileData.rotationX -= 90;
        }
        else if(tileData.rotationY==270){
            tileData.rotationZ -= 90;
        }
        int current1 = tileData.top;
        tileData.top = tileData.left;
        tileData.left = tileData.bottom;
        tileData.bottom = tileData.right;
        tileData.right = current1;
    }

    public void rotateZ90(){
        if(tileData.rotationY==0){
            tileData.rotationZ += 90;
        }
        else if(tileData.rotationY==90){
            tileData.rotationX -= 90;
        }
        else if(tileData.rotationY==180){
            tileData.rotationZ -= 90;
        }
        else if(tileData.rotationY==270){
            tileData.rotationX += 90;
        }
        int current2 = tileData.top;
        tileData.top = tileData.front;
        tileData.front = tileData.bottom;
        tileData.bottom = tileData.back;
        tileData.back = current2;
    }

    int ReverseInteger(int input)
    {
        string inputString = input.ToString();
        char[] charArray = inputString.ToCharArray();
        System.Array.Reverse(charArray);
        string reversedString = new string(charArray);
        int reversedInteger;
        int.TryParse(reversedString, out reversedInteger);
        return reversedInteger;
    }

}