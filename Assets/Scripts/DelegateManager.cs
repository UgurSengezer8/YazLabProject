using UnityEngine;

public class DelegateManager : MonoBehaviour
{

    public delegate void EmptyDelegate();

    private EmptyDelegate emptyDelegateFunction;

    private void Start()
    {
        //emptyDelegateFunction = MyFirstEmptyDelegateFunction;
        emptyDelegateFunction = () =>
        {
            MyFirstEmptyDelegateFunction();
        };
        emptyDelegateFunction += delegate 
        {
            MySecondEmptyDelegateFunction();
        };
        emptyDelegateFunction();
        //emptyDelegateFunction += MySecondEmptyDelegateFunction;
        //
        //emptyDelegateFunction();
    }

    public void MyFirstEmptyDelegateFunction()
    {
        Debug.Log("myfirst");
    }
    public void MySecondEmptyDelegateFunction()
    {
        Debug.Log("mysecond");
    }
}
