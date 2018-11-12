﻿using Assets.Scripts;
using UnityEngine;


// uses algorithm from https://gamedev.stackexchange.com/questions/27056/how-to-achieve-uniform-speed-of-movement-on-a-bezier-curve
public class PathFollower : MonoBehaviour
{
    public enum PathType { Ground, Air }

    public PathType Type;
    public float Speed;
    public float OffsetAmount;

    private Vector2 PositionOffset;
    private Path path;
    private int segmentIndex;

    private Vector2 A, B, C, D;
    private Vector2 v1, v2, v3;
    private float t;

	void OnEnable ()
    {
        switch(Type)
        {
            case PathType.Ground: path = GameObject.Find("GroundPath").GetComponent<PathCreator>().path; break;
            case PathType.Air: path = GameObject.Find("AirPath").GetComponent<PathCreator>().path; break;
        }

        segmentIndex = 0;

        if (PositionOffset == null)
        {
            PositionOffset = Random.insideUnitCircle * Random.Range(-OffsetAmount, OffsetAmount);
        }

        RecomputeSegment();
        transform.position = A;        
	}
	
	void FixedUpdate ()
    {
        if (segmentIndex >= path.NumSegments) return;

        if(t >= 1.0f)
        {
            segmentIndex++;
            if (segmentIndex >= path.NumSegments) return;

            RecomputeSegment();
        }

        var L = Time.deltaTime * Speed;
        var tangent = t * t * v1 + t * v2 + v3;

        for (int i = 0; i < 100; i++)
        {
            t = t + (L / 100) / tangent.magnitude;
            tangent = t * t * v1 + t * v2 + v3;
        }

        transform.position = Bezier.EvaluateCubic(A, B, C, D, t);

        transform.eulerAngles = new Vector3(0.0f, 0.0f, MathHelpers.Angle(tangent, Vector2.right));
    }

    private void RecomputeSegment()
    {
        var segment = path.GetPointsInSegment(segmentIndex);

        A = segment[0] + (Vector2)PositionOffset;
        B = segment[1] + (Vector2)PositionOffset;
        C = segment[2] + (Vector2)PositionOffset;
        D = segment[3] + (Vector2)PositionOffset;

        v1 = -3 * A + 9 * B - 9 * C + 3 * D;
        v2 = 6 * A - 12 * B + 6 * C;
        v3 = -3 * A + 3 * B;

        t = 0;
    }
}
