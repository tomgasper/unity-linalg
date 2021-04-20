using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class Gram_Schmidt : MonoBehaviour
{
    /*  A matrix:         Q matrix:
        [       ]     [               ]
        [ i j k ]     [  q_1 q_2 q_3  ]
        [       ]     [               ]  
    */
    List<float> A = new List<float>();
    List<float> Q = new List<float>();

    Vector3 i = new Vector3(1.0f, 1.2f, 2.0f);
    Vector3 j = new Vector3(0.3f, -1.0f, 2.1f);
    Vector3 k = new Vector3(0.0f, 0.4f, -2.2f);

    // Projection and error of j
    Vector3 p_j;
    Vector3 e_j;

    // Projection and error of k
    Vector3 p_k;
    Vector3 e_k;

    // Unity objects used to specify i, j, k vector length and direction
    public GameObject s1;
    public GameObject s2;
    public GameObject s3;

    void GS()
    {
        Vector3 q_1;
        Vector3 q_2;
        Vector3 q_3;

        // Project b onto the a, get proj_b (a) error, that's 90 deg vector
        // and assign it to B
        // error = b - (i(transpose)j / i(transpose)i * i)

        float scalar = (i.x * j.x + i.y * j.y + i.z * j.z)/
                       (i.x * i.x + i.y * i.y + i.z * i.z);

        // Calculate our projection from vector j to vector i
        p_j = new Vector3(scalar * i.x, scalar*i.y, scalar * i.z);
        e_j = j - p_j;

        // Calculate projection from c to the plane spanned by i and j
        // so error2 = c - ( i(transpose)k / i(transpose)i * i )

        float scalar_b = (e_j.x * k.x + e_j.y * k.y + e_j.z * k.z) /
                         (e_j.x * e_j.x + e_j.y * e_j.y + e_j.z * e_j.z);

        float scalar_c = (i.x * k.x + i.y * k.y + i.z * k.z) /
                         (i.x * i.x + i.y * i.y + i.z * i.z);

        p_k = new Vector3(scalar_b * e_j.x + scalar_c * i.x,
                          scalar_b * e_j.y + scalar_c * i.y,
                          scalar_b * e_j.z + scalar_c * i.z);
        e_k = k - p_k;

        // Convert to unit vectors
        q_1 = i   / Mathf.Sqrt(i.x * i.x + i.y * i.y + i.z * i.z);
        q_2 = e_j / Mathf.Sqrt(e_j.x * e_j.x + e_j.y * e_j.y + e_j.z * e_j.z);
        q_3 = e_k / Mathf.Sqrt(e_k.x * e_k.x + e_k.y * e_k.y + e_k.z * e_k.z);

        /*q_1 = i;
        q_2 = e_j;
        q_3 = e_k;*/

        if (s3 != null)
        {
            visual_LinesToShapes();
        }

        // Update Unity lists
        // Update Q matrix
        Q.Clear();
        Q.AddRange(
            new List<float>
            {
                q_1.x, q_2.x, q_3.x,
                q_1.y, q_2.y, q_3.y,
                q_1.z, q_2.z, q_3.z
            }
          );
    }

    // factor any m by n matrix with indepedent columns into QR
    void QR_factorization()
    {
        // Q matrix - normalized orthogonal columns
        // R matrix - upper triangular with positive diagonal

        // 1 - Get reverse matrix R
        // i,j entry of R is a dot product of q_i and a_j

        List<float> R = new List<float>();

        float n = 3;
        float m = 3;

        List<float> Q_t = helper_Transpose(Q);

        for (int i=0; i < m; i++)
        {
            // 0-2 1st row,
            // 3-5 2nd row
            // 6-8 3rd row
            for (int j = 0; j < n; j++)
            {
                float r = (Q_t[3*i]*A[j] )+(Q_t[3*i+1]*A[j+3])+(Q_t[3*i+2]*A[j+6]);
                R.Add((float)Math.Round(r,2));
            }
        }
        Debug.Log(
            R[0].ToString() + " " + R[1].ToString() + " " + R[2].ToString() + "\n" +
            R[3].ToString() + " " + R[4].ToString() + " " + R[5].ToString() + "\n" +
            R[6].ToString() + " " + R[7].ToString() + " " + R[8].ToString()
            );

        List<float> isA = helper_MultMatrix(Q, R);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (s1 != null && s2 != null && s3 != null)
        {
            i = s1.transform.position;
            j = s2.transform.position;
            k = s3.transform.position;

            // Update A matrix based on input
            A.Clear();
            A.AddRange(
                new List<float>
                {
                    i.x, j.x, k.x,
                    i.y, j.y, k.y,
                    i.z, j.z, k.z
                });
        }

        if ( Q.Count >= 8 )
        {
            // Create vector lines based on generated Q matrix
            Debug.DrawLine(Vector3.zero, new Vector3(Q[0], Q[3], Q[6]));
            Debug.DrawLine(Vector3.zero, new Vector3(Q[1], Q[4], Q[7]));
            Debug.DrawLine(Vector3.zero, new Vector3(Q[2], Q[5], Q[8]));
        }

        GS();
        QR_factorization();
    }

    void visual_Projection(Vector3 start_e1, Vector3 e1, Vector3 start_e2, Vector3 e2)
    {
        Debug.DrawLine(start_e1, e1, Color.blue);
        Debug.DrawLine(start_e2, e2, Color.blue);
    }

    void visual_LinesToShapes()
    {
        if (s1 != null && s2 != null && s3 != null)
        {
            Debug.DrawLine(Vector3.zero, s1.transform.position, Color.white);
            Debug.DrawLine(Vector3.zero, s2.transform.position, Color.yellow);
            Debug.DrawLine(Vector3.zero, s3.transform.position, Color.yellow);
        }
    }

    List<float> helper_MultMatrix(List<float> m1, List<float> m2)
    {
        List<float> r_m = new List<float>();

        float m = 3;
        float n = 3;

        for (int i = 0; i < m; i++)
        {
            // 0-2 1st row,
            // 3-5 2nd row
            // 6-8 3rd row
            for (int j = 0; j < n; j++)
            {
                float r = (m1[3 * i] * m2[j]) + (m1[3 * i + 1] * m2[j + 3]) + (m1[3 * i + 2] * m2[j + 6]);
                r_m.Add((float)Math.Round(r, 2));
            }
        }
        return r_m;
    }

    List<float> helper_Transpose(List<float> m)
    {
        List<float> t_m = new List<float>
        {
            m[0],m[3],m[6],
            m[1],m[4],m[7],
            m[2],m[5],m[8]
        };

        return t_m;
    }
}
