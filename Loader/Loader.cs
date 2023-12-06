using System.Collections.Generic;
using UniRx;

public class Loader
{
    public ReactiveProperty<float> status = new ReactiveProperty<float>();
    public ReactiveProperty<bool> onCompleted = new ReactiveProperty<bool>();
    public Subject<string> onError = new Subject<string>();

    private Queue<LoaderElement> queue = new Queue<LoaderElement>();
    private Dictionary<LoaderElement, bool> isAsyncData = new Dictionary<LoaderElement, bool>();
    private int expectedLoads = 0;
    private int loadsLeftCount = 0;

    private int internalExpectedLoads = 0;

    public void Add(LoaderElement newLoaderElement, bool loadAsyncWithNextOne = false)
    {
        queue.Enqueue(newLoaderElement);
        isAsyncData.Add(newLoaderElement, loadAsyncWithNextOne);
    }

    public void StartLoad()
    {
        expectedLoads = queue.Count;
        loadsLeftCount = queue.Count;
        Next();
    }

    private void Next()
    {
        if (queue.Count == 0)
        {
            status.Value = 1;
            onCompleted.Value = true;
            return;
        }

        // Each time we create a list of all the next async elements so they can be called together
        List<LoaderElement> batch = new List<LoaderElement>();
        while (true)
        {
            if (queue.Count == 0)
            {
                break;
            }

            LoaderElement le = queue.Dequeue();
            batch.Add(le);

            if (!isAsyncData[le])
            {
                break;
            }
        }

        internalExpectedLoads = batch.Count;
        foreach (var itemToLoad in batch)
        {
            itemToLoad.Load(
                () =>
                {
                    OnSingleLoadDone();
                },
                OnError);
        }
    }

    private void OnSingleLoadDone()
    {
        internalExpectedLoads--;
        loadsLeftCount--;

        status.Value = 1f - ((float)loadsLeftCount / expectedLoads);

        if (internalExpectedLoads == 0)
        {
            Next();
        }
    }

    private void OnError(string errorMessage)
    {
        onError.OnNext(errorMessage);
    }
}
