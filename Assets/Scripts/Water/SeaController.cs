using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using TreeEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Color = UnityEngine.Color;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SeaController : MonoBehaviour
{
    public ComputeShader SeaComputeShader;
    
    public int SeaSize;            //Size in meters
    public float SeaResolution;    //Number of quad per meter 
    public Material SeaMaterial;
    public float WaveHeight;
    public float WaveLength;
    public float WaveSpeed;
    public Vector2 WaveDirection;
    public float Steepness = 1;
    private static Vector3[] _vertices;
    private static Vector3[] _normals;
    private Vector2[] _uvs;
    private int[] _triangles;
    

    
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Mesh _mesh;
    

    private static int _verticesPerSide;
    private int _triangleIndex = 0;

    private ComputeBuffer _verticesBuffer;
    private int _updateKernelHandle;
    

    private static bool _destroy;
    
    // Start is called before the first frame update
    void Start()
    {
        _destroy = false;

        _mesh = new Mesh {indexFormat = IndexFormat.UInt32};

        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        _meshFilter.mesh = _mesh;
        _meshRenderer.material = SeaMaterial;
         
        
        
        _verticesPerSide  = SeaSize  + 1;

        _vertices = new Vector3[_verticesPerSide * _verticesPerSide];     
        _normals = new Vector3[_verticesPerSide * _verticesPerSide];     
        _uvs = new Vector2[_verticesPerSide * _verticesPerSide];     
        _triangles = new int[SeaSize * SeaSize * 6];

        SeaComputeShader.SetInt("SeaSize", SeaSize);
        SeaComputeShader.SetFloat("SeaResolution", SeaResolution);
        SeaComputeShader.SetInt("VerticesPerSide", _verticesPerSide);
        
        _verticesBuffer = new ComputeBuffer(_verticesPerSide * _verticesPerSide, 12);
        ComputeBuffer uvBuffer = new ComputeBuffer(_verticesPerSide * _verticesPerSide, 8);
        ComputeBuffer triangleBuffer = new ComputeBuffer(SeaSize * SeaSize * 6, sizeof(int));


        int generateMeshHandle = SeaComputeShader.FindKernel("GenerateMesh");
        _updateKernelHandle = SeaComputeShader.FindKernel("UpdateMesh");

        SeaComputeShader.SetBuffer(generateMeshHandle, "Vertices", _verticesBuffer);       
        SeaComputeShader.SetBuffer(generateMeshHandle, "Triangles", triangleBuffer);
        SeaComputeShader.SetBuffer(generateMeshHandle, "Uvs", uvBuffer);
       
        
        SeaComputeShader.Dispatch(generateMeshHandle, 1, 1, 1);

        _verticesBuffer.GetData(_vertices);
        triangleBuffer.GetData(_triangles);
        uvBuffer.GetData(_uvs);
        
        triangleBuffer.Dispose();

        SeaComputeShader.SetBuffer(_updateKernelHandle, "ActiveVertices", _verticesBuffer);
        SeaComputeShader.SetInt( "SizePerSide", _verticesPerSide);
        
       
        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uvs;
        
        uvBuffer.Dispose();
        UpdateMesh();
    }
    
    
    private void UpdateMesh()
    {
        SeaComputeShader.Dispatch(_updateKernelHandle, _verticesPerSide, _verticesPerSide, 1);
        AsyncGPUReadback.Request(_verticesBuffer, SetPoints);
      _mesh.RecalculateNormals();
      _normals = _mesh.normals;
    }

    private void SetPoints(AsyncGPUReadbackRequest obj)
    {
        if(_destroy)
            return;
        
        _vertices = obj.GetData<Vector3>().ToArray();
        _mesh.vertices = _vertices;

    }
/*
    private void SetNormals()
    {
        if(_destroy)
            return;
        
        _normalBuffer.GetData(_normals);
        for (int i = 0; i < _normals.Length; i++)
        {
            _normals[i] = _normals[i].normalized;
           // Debug.DrawRay(_vertices[i], _normals[i], Color.red);
        }
        
        _mesh.normals = _normals;

        for (int i = 0; i < _normals.Length; i++)
        {
            _normals[i] = Vector3.zero;
        }
        
        _normalBuffer.SetData(_normals);
    }

*/
    void Update()
    {      
        SeaComputeShader.SetFloat("WaveLength", WaveLength);
        SeaComputeShader.SetFloat("Amplitude", WaveHeight);
        SeaComputeShader.SetFloat("Speed", WaveSpeed);
        SeaComputeShader.SetVector("Direction", WaveDirection);
        SeaComputeShader.SetFloat("Steepness", Steepness);
        SeaComputeShader.SetFloat("Time", Time.time);

        UpdateMesh();
  //      Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, SeaMaterial, 0, null, 10);
    }

    public static float GetHeight(Vector3 pos)
    {
        Vector2Int intPos = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        intPos += new Vector2Int(_verticesPerSide / 2, _verticesPerSide / 2);
        return _vertices[intPos.y * _verticesPerSide + intPos.x].y;
    }
    public static Vector3 GetSurfaceNormal(Vector3 pos)
    {
        Vector2Int intPos = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        intPos += new Vector2Int(_verticesPerSide / 2, _verticesPerSide / 2);
        return _normals[intPos.y * _verticesPerSide + intPos.x];
    }

    private void OnDestroy()
    {
        _verticesBuffer.Release();
        _destroy = true;
    }
}
