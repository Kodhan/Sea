using System;
using UnityEngine;

public class Buoyancy : MonoBehaviour
{

    [SerializeField]private float _radius = 1;
    [SerializeField]private float _density = 1;
    private float _depth;
    private static readonly float RHO_GRAVITY = Physics.gravity.magnitude * 1000f;  //rho = density of water = 1000 Kg/m3 , gravity = 10 m/s2
    private Rigidbody _rigidbody;
    private const float _densityOfAir = 1.2754f; 
    private const float _densityOfWater = 997f; 
    private const float _sphereDragCoefficient = 0.47f;
    void Start()
    {
        _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.mass = GetVolume(_radius) * _density;
    }
    
    void FixedUpdate()
    {
        float buoyancyForce = RHO_GRAVITY * GetSubmersedVolume(_radius);
        CalculateDrag();
        //Debug.Log(buoyancyForce);
        _rigidbody.AddForce(Vector3.up * buoyancyForce, ForceMode.Force);
        
    }

    private float GetSubmersedVolume(float radius)
    {
        _depth = radius - transform.position.y + SeaController.GetHeight(transform.position);
        _depth = Mathf.Clamp(_depth, 0f, radius * 2);
        return Mathf.PI * ((radius * (_depth * _depth)) - (_depth * _depth * _depth) / 3f) ;
    }

    private void CalculateDrag()
    {
     
        //Do air drag
        if ((transform.position.y - _radius )- SeaController.GetHeight(transform.position) > 0)
        {
            float dragForce = (_densityOfAir / 2) * _rigidbody.velocity.magnitude * _rigidbody.velocity.magnitude * _sphereDragCoefficient * _radius * _radius * Mathf.PI;
            _rigidbody.AddForce(dragForce * (-_rigidbody.velocity.normalized));
        }
        //Do Water drag
        else
        {
            float dragForce = (_densityOfWater / 2) * _rigidbody.velocity.magnitude * _rigidbody.velocity.magnitude * _sphereDragCoefficient * _radius * _radius * Mathf.PI;
            Debug.Log(dragForce);
            _rigidbody.AddForce(dragForce * (-_rigidbody.velocity.normalized));
        }
    }

    private float GetVolume(float radius)
    {
        return (4f/3f) * Mathf.PI * radius * radius * radius;
    }
}
