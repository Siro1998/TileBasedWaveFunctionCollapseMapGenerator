using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile3DS
{
    public TileData3DS tileData;
    [HideInInspector]
    public List<Tile3DS> frontNeighbors;
    [HideInInspector]
    public List<Tile3DS> rightNeighbors;
    [HideInInspector]
    public List<Tile3DS> backNeighbors;
    [HideInInspector]
    public List<Tile3DS> leftNeighbors;
    [HideInInspector]
    public List<Tile3DS> topNeighbors;
    [HideInInspector]
    public List<Tile3DS> bottomNeighbors;

    public Tile3DS(TileData3DS tileData){
        this.tileData = new TileData3DS(tileData.tileObject, tileData.rotation);
        this.tileData.front[0] = tileData.front[0];
        this.tileData.back[0] = tileData.back[0];
        this.tileData.left[0] = tileData.left[0];
        this.tileData.right[0] = tileData.right[0];
        this.tileData.top[0] = tileData.top[0];
        this.tileData.bottom[0] = tileData.bottom[0];
        this.tileData.front[1] = tileData.front[1];
        this.tileData.back[1] = tileData.back[1];
        this.tileData.left[1] = tileData.left[1];
        this.tileData.right[1] = tileData.right[1];
        this.tileData.top[1] = tileData.top[1];
        this.tileData.bottom[1] = tileData.bottom[1];
        frontNeighbors = new List<Tile3DS>();
        rightNeighbors = new List<Tile3DS>();
        backNeighbors = new List<Tile3DS>();
        leftNeighbors = new List<Tile3DS>();
        topNeighbors = new List<Tile3DS>();
        bottomNeighbors = new List<Tile3DS>();
    }


    public void analyze(List<Tile3DS> tiles){
        for(int i=0; i< tiles.Count; i++){
            Tile3DS tile = tiles[i];
            if(tile.tileData.front[0] == this.tileData.back[0] && ReverseString(tile.tileData.front[1]) == this.tileData.back[1]){
                this.backNeighbors.Add(tile);
            }
            if(tile.tileData.right[0] == this.tileData.left[0] && ReverseString(tile.tileData.right[1]) == this.tileData.left[1]){
                this.leftNeighbors.Add(tile);
            }
            if(tile.tileData.left[0] == this.tileData.right[0] && ReverseString(tile.tileData.left[1]) == this.tileData.right[1]){
                this.rightNeighbors.Add(tile);
            }
            if(tile.tileData.back[0] == this.tileData.front[0] && ReverseString(tile.tileData.back[1]) == this.tileData.front[1]){
                this.frontNeighbors.Add(tile);
            }
            if(tile.tileData.bottom[0] == this.tileData.top[0] && ReverseString(tile.tileData.bottom[1]) == this.tileData.top[1]){
                this.topNeighbors.Add(tile);
            }
            if(tile.tileData.top[0] == this.tileData.bottom[0] && ReverseString(tile.tileData.top[1]) == this.tileData.bottom[1]){
                this.bottomNeighbors.Add(tile);
            }
        }
    }
    public void rotateY90(){
        tileData.rotation.y += 90;
        string[] current = new string[2];
        current[0] = tileData.front[0];
        current[1] = tileData.front[1];
        tileData.front[0] = tileData.right[0];
        tileData.front[1] = tileData.right[1];
        tileData.right[0] = tileData.back[0];
        tileData.right[1] = tileData.back[1];
        tileData.back[0] = tileData.left[0];
        tileData.back[1] = tileData.left[1];
        tileData.left[0] = current[0];
        tileData.left[1] = current[1];
        
        string temptop0 = tileData.top[0];
        tileData.top[0] = tileData.top[1];
        tileData.top[1] = temptop0;
        tileData.top[1] = ReverseString(tileData.top[1]);

        string tempbottom0 = tileData.bottom[0];
        tileData.bottom[0] = tileData.bottom[1];
        tileData.bottom[0] = ReverseString(tileData.bottom[0]);
        tileData.bottom[1] = tempbottom0;
    }
    public void rotateX90(){
        if(tileData.rotation.y==0){
            tileData.rotation.x += 90;
        }
        else if(tileData.rotation.y==90){
            tileData.rotation.z += 90;
        }
        else if(tileData.rotation.y==180){
            tileData.rotation.x -= 90;
        }
        else if(tileData.rotation.y==270){
            tileData.rotation.z -= 90;
        }
        string[] current1 = new string[2];
        current1[0] = tileData.top[0];
        current1[1] = tileData.top[1];
        tileData.top[0] = tileData.left[1];
        tileData.top[1] = tileData.left[0];
        tileData.top[1] = ReverseString(tileData.top[1]);

        tileData.left[0] = tileData.bottom[1];
        tileData.left[0] = ReverseString(tileData.left[0]);
        tileData.left[1] = tileData.bottom[0];

        tileData.bottom[0] = tileData.right[1];
        tileData.bottom[0] = ReverseString(tileData.bottom[0]);
        tileData.bottom[1] = tileData.right[0];

        tileData.right[0] = current1[1];
        tileData.right[1] = current1[0];
        tileData.right[1] = ReverseString(tileData.right[1]);
        
        string tempfront0 = tileData.front[0];
        tileData.front[0] = tileData.front[1];
        tileData.front[1] = tempfront0;
        tileData.front[1] = ReverseString(tileData.front[1]);

        string tempback0 = tileData.back[0];
        tileData.back[0] = tileData.back[1];
        tileData.back[0] = ReverseString(tileData.back[0]);
        tileData.back[1] = tempback0;
    }

    public void rotateZ90(){
        if(tileData.rotation.y==0){
            tileData.rotation.z += 90;
        }
        else if(tileData.rotation.y==90){
            tileData.rotation.x -= 90;
        }
        else if(tileData.rotation.y==180){
            tileData.rotation.z -= 90;
        }
        else if(tileData.rotation.y==270){
            tileData.rotation.x += 90;
        }
        string[] current2 = new string[2];
        current2[0] = tileData.top[0];
        current2[1] = tileData.top[1];
        tileData.top[0] = tileData.front[0];
        tileData.top[1] = tileData.front[1];
        tileData.front[0] = ReverseString(tileData.bottom[0]);
        tileData.front[1] = ReverseString(tileData.bottom[1]);
        tileData.bottom[0] = tileData.back[0];
        tileData.bottom[1] = tileData.back[1];
        tileData.back[0] = ReverseString(current2[0]);
        tileData.back[1] = ReverseString(current2[1]);

        string templeft0 = tileData.left[0];
        tileData.left[0] = tileData.left[1];
        tileData.left[0] = ReverseString(tileData.left[0]);
        tileData.left[1] = templeft0;

        string tempright0 = tileData.right[0];
        tileData.right[0] = tileData.right[1];
        tileData.right[1] = tempright0;
        tileData.right[1] = ReverseString(tileData.right[1]);
    }

    string ReverseString(string input){
        char[] charArray = input.ToCharArray();
        System.Array.Reverse(charArray);
        string reversedString = new string(charArray);
        return reversedString;
    }

}