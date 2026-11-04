using UnityEngine;
using UnityEngine.AI;

public class PushTaskExecutor : MonoBehaviour
{
    /*
    [Header("Referanslar")]
    [SerializeField] private NavMeshAgent characterAgent;
    [SerializeField] private RobotMovement rm;

    [SerializeField] private Transform boxTransform;
    [SerializeField] private float pushSpeed = 2.5f;

    // Kuyruğu işlerken üst üste çalışma olmaması için bayrak
    private bool isProcessingQueue = false;

    /// <summary>
    /// Kuyrukta görev varsa sırayla çalıştırmayı başlatır.
    /// </summary>
    public void ProcessQueue()
    {
        if (!isProcessingQueue && rm..Count > 0)
        {
            StartCoroutine(ExecuteTasksRoutine());
        }
    }

    private IEnumerator ExecuteTasksRoutine()
    {
        isProcessingQueue = true;

        while (SRQueue.Count > 0)
        {
            // 1. Kuyruğun en başındaki görevi al
            PushTask currentTask = SRQueue.Dequeue();

            // ------------------------------------------------------------------
            // AŞAMA 1: Karakterin Kutunun Arkasına (robotStartingPosition) Gitmesini Bekle
            // ------------------------------------------------------------------
            characterAgent.isStopped = false;
            characterAgent.SetDestination(currentTask.robotStartingPosition);

            // Yolu hesaplamasını ve hedefe ulaşmasını bekle
            while (characterAgent.pathPending || characterAgent.remainingDistance > 0.1f)
            {
                yield return null; // Bir sonraki kareye kadar bekle
            }

            // Karakter başlangıç konumuna ulaştı, durdur ve açısını itme yönüne çevir
            characterAgent.velocity = Vector3.zero;
            if (currentTask.pushDirection != Vector3.zero)
            {
                characterAgent.transform.rotation = Quaternion.LookRotation(currentTask.pushDirection);
            }

            // ------------------------------------------------------------------
            // AŞAMA 2: Karakterin Kutuyu İterek Hedefe (robotEndingPosition) Gitmesini Bekle
            // ------------------------------------------------------------------
            // Karakteri hedef itme noktasına yönlendir
            characterAgent.SetDestination(currentTask.robotEndingPosition);

            // Kutu hedef noktaya tam varana kadar her karede kutuyu ve karakteri ilerlet
            while (Vector3.Distance(boxTransform.position, currentTask.robotEndingPosition) > 0.05f)
            {
                // Kutuyu hedef noktaya doğru hareket ettir
                boxTransform.position = Vector3.MoveTowards(
                    boxTransform.position,
                    currentTask.robotEndingPosition,
                    pushSpeed * Time.deltaTime
                );

                yield return null; // Bir sonraki kareye geç
            }

            // Kutunun pozisyonunu hedef noktaya tam kilitle (Snap)
            boxTransform.position = currentTask.robotEndingPosition;
        }

        isProcessingQueue = false;
        Debug.Log("Kuyruktaki tüm itme görevleri başarıyla tamamlandı.");
    }*/
}
