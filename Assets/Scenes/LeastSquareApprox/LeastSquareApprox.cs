using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeastSquareApprox : MonoBehaviour
{
    public LineRenderer LineRenderer;
    public GameObject point_prefab;
    private List<Vector2> points_data = new List<Vector2>();
    private List<float> line_params = new List<float>();
    private Vector2 last_point = new Vector2();
    private Vector2 first_point = new Vector2();
    public int m = 10;


        // least square approx
    // ideally we would want x(C,D params of a line) that solves Ax=b
    // where A is collection of C constant + our times for y

    // but Ax=b most likely isnt solvable so we need to use
    // least square approx

    // A(T)A(x_appro) = A(T)b

    // Lets create matrix form A for our data first

    //  [ 1 t_1 ]    [   ]
    //  [ 1 t_2 ]  = [ A ]
    //  [ 1 t_m ]    [   ]

    //           
    //  A(T)b =  [  sum_i=1_m( b_m )    ]
    //           [ sum_i=1_m( t_i*b_i ) ]

    //  A(T)A =  [        m         sum_i=1_m( t_m )     ]
    //           [ sum_i=1_m( t_m ) sum_i=1_m( t_m*t_m ) ]

    private List<float> leastSquareApprox(List<Vector2> points)
    {
        // lets form A first

        List<Vector2> A = new List<Vector2>();

        for (int i = 0; i < points.Count; i++)
        {
            // x is our time
            // creating entry [ 1  t_i ]
            A.Add(new Vector2(1, points[i].x));
        }

        // solve A(T)A

        List <float> A_TA = new List<float>();

        // first entry is just sum of all 1's so lets just use length of A ;)
        A_TA.Add(A.Count);
        float t_sum = 0;
        float t_square_sum = 0;

        for (int i = 0; i < A.Count; i++)
        {
            // note that y here is our time(x) in fact
            t_sum += A[i].y;
            t_square_sum += A[i].y*A[i].y;
        }

        // fill rest of our matrix
        A_TA.Add(t_sum);
        A_TA.Add(t_sum);
        A_TA.Add(t_square_sum);

        // solve A(T)b

        List<float> A_Tb = new List<float>();
        float b_sum = 0;
        float t_b_sum = 0;

        for (int i = 0; i < A.Count; i++)
        {
            // note that y here is our time(x) in fact
            b_sum += points[i].y;
            t_b_sum += points[i].x * points[i].x;
        }

        A_Tb.Add(b_sum);
        A_Tb.Add(t_b_sum);

        // noob way of solving simple lin equations :D
        // switch equations if first pivot is 0

        float up_1 = A_TA[0] == 0 ? A_TA[0] : A_TA[2];
        float up_2 = A_TA[0] == 0 ? A_TA[1] : A_TA[3];
        float up_3 = A_TA[0] == 0 ? A_Tb[0] : A_Tb[1];

        float bt_1 = A_TA[0] == 0 ? A_TA[2] : A_TA[0];
        float bt_2 = A_TA[0] == 0 ? A_TA[3] : A_TA[1];
        float bt_3 = A_TA[0] == 0 ? A_Tb[1] : A_Tb[0];

        float D;
        float C;

        // cant divide by zero!
        if (bt_1 != 0)
        {
            float p_scl = up_1/bt_1;
            D = (p_scl * bt_3 - up_3)/(p_scl * bt_2 - up_2);
        }
        else
        {
            D = bt_3 / bt_2;
        }

        C = (up_3 - up_2 * D)/up_1;
        
        

        List<float> result = new List<float>{C,D};

        Debug.Log(A_TA[0] + "C" + " + " + A_TA[1] + "D" + " = " + A_Tb[0] );
        Debug.Log(A_TA[2] + "C" + " + " + A_TA[3] + "D" + " = " + A_Tb[1] );
        Debug.Log(result[0] + "  " + result[1]);

        return result;
    }

    private void generateRandom(GameObject prefab, int m)
    {
        for (int i = 0; i <= m; i++)
        {
            Vector3 spawnPoint = new Vector3(Random.Range(-5, 5), Random.Range(-2,3), 0);
            GameObject p = Instantiate(prefab, spawnPoint, Quaternion.identity);
            Vector2 p_t = new Vector2( p.transform.position.x, p.transform.position.y);

            // register point that has the bigest x
            // this will be our times cap
            if (points_data.Count > 0)
            {
                if (p_t.x > points_data[points_data.Count-1].x && p_t.x > last_point.x)
                {
                last_point = p_t;
                }
                else if (p_t.x < points_data[points_data.Count-1].x && p_t.x < first_point.x)
                {
                    first_point = p_t;
                }

            }
            else
            {
                first_point = p_t;
                last_point = p_t;
            }

            points_data.Add(p_t); 
        }
        
    }
    void drawApproxLine(float t_s, float t_e, List<float> l_params)
    {
        // line -> C + Dt = y
        float C = l_params[0];
        float D = l_params[1];

        float p1_y = C + D*t_s;
        float p2_y = C + D*t_e;

        Vector3 p1 = new Vector3(t_s, p1_y, 0);
        Vector3 p2 = new Vector3(t_e, p2_y, 0);

        LineRenderer.SetPosition(0, p1);
        LineRenderer.SetPosition(1, p2);
    }

    void Start()
    {
        generateRandom(point_prefab, m);
        line_params = leastSquareApprox(points_data);

        if (line_params != null && line_params.Count == 2)
        {
            // and this is the line that minimazies square errors of entry points
            drawApproxLine(first_point.x, last_point.x, line_params);
        }
    }

}
