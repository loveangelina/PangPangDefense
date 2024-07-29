using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Match3Controller : MonoBehaviour
{
    // TODO : 보드의 길이로 바꾸기
    public int width;
    public int height;
    public Sprite[] itemPrefabs; // 아이템 프리팹 배열
    public GameObject board;        // 보드 판
    public Camera boardCamera;

    private BoardItem[] items;    // 보드 위에 놓이는 아이템 배열
    private GameObject[] itemObjects;   // items와 연계되는 보드 위 오브젝트
    private GameObject firstClickedItem = null;

    void Start()
    {
        items = new BoardItem[width * height];
        itemObjects = new GameObject[width * height];
        
        SetupBoard();
    }

    void SetupBoard()
    {
        SpriteRenderer[] boardGrid = board.GetComponentsInChildren<SpriteRenderer>();
        for(int i = 0; i < boardGrid.Length; i++)
        {
            items[i] = new BoardItem(boardGrid[i], i);
            items[i].spriteRenderer.sprite = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
            itemObjects[i] = boardGrid[i].gameObject;
        }
    }
    
    void Update()
    {
        // 마우스 클릭 처리
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = boardCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                GameObject clickedItem = hitInfo.collider.gameObject;

                // 클릭된 객체가 아이템인지 확인
                if (IsItem(clickedItem))
                {
                    firstClickedItem = clickedItem;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && firstClickedItem != null)
        {
            Ray ray = boardCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                // tODO : 각 item에 넣을 스크립트에 index 정보 넣기 
                GameObject secondClickedItem = hitInfo.collider.gameObject;
                Debug.Log("첫번째클릭 : " + firstClickedItem.name);
                Debug.Log("두번째클릭 : " + secondClickedItem.name);

                // 클릭된 객체가 아이템인지 확인
                // TODO isitem으로 조건체크가 아닌, dictionary에 해당 게임오브젝트가 있는지로 확인?
                if (IsItem(secondClickedItem) && secondClickedItem != firstClickedItem)
                {
                    int indexA = GetIndex(firstClickedItem);
                    int indexB = GetIndex(secondClickedItem);
                    
                    // 두 아이템의 위치 스왑
                    Debug.Log($"{indexA}:{items[indexA].spriteRenderer.sprite.name}와 {indexB}:{items[indexB].spriteRenderer.sprite.name}를 바꿈");
                    SwapItems(indexA, indexB);

                    // 매치된 아이템 찾기
                    List<BoardItem> matchingItemsA = FindMatchingItems(items[indexA]);
                    List<BoardItem> matchingItemsB = FindMatchingItems(items[indexB]);

                    // 중복 매치 제거
                    /*HashSet<BoardItem> uniqueMatches = new HashSet<BoardItem>(matchingItemsA);
                    uniqueMatches.UnionWith(matchingItemsB);

                    Debug.Log("매칭된 아이템 개수 : " + matchingItemsA.Count + " / " +  matchingItemsB.Count);

                    // 매치된 아이템이 3개 이상이면 제거
                    if (uniqueMatches.Count >= 3)
                    {
                        foreach (var item in uniqueMatches)
                        {
                            // 아이템 제거 처리 (비활성화 예시)
                            itemObjects[item.index].SetActive(false);
                        }

                        // 보드 재구성
                        FillEmptySpaces();
                    }
                    else
                    {
                        // TODO : 매치가 안 되면 원래 위치로 되돌리기
                        SwapItems(indexA, indexB);
                    }*/
                }
            }

            // 첫 번째 아이템 초기화
            firstClickedItem = null;
        }
    }

    // TODO : collider 말고 아이템 전용 스크립트 ㅂ만들어서 넣기 
    bool IsItem(GameObject clickedItem)
    {
        // obj가 아이템인지 여부를 체크하는 로직
        // 여기에서는 간단히 Collider로 판단
        
        // clickedItem 게임오브젝트에 해당하는 게 배열에 있을 경우 해당 값이 처음 나타나는 인덱스를 반환, 못 찾으면 -1 반환
        return System.Array.IndexOf(itemObjects, clickedItem) >= 0;
    }
    
    int GetIndex(GameObject item)
    {
        for (int i = 0; i < itemObjects.Length; i++)
        {
            if (itemObjects[i] == item)
            {
                return i;
            }
        }
        return -1;
    }

    List<BoardItem> FindMatchingItems(BoardItem clickedItem)
    {
        List<BoardItem> matchingItems = new List<BoardItem>();
        Queue<BoardItem> toCheck = new Queue<BoardItem>();
        HashSet<BoardItem> visited = new HashSet<BoardItem>();

        // 초기 큐에 클릭된 아이템 추가
        toCheck.Enqueue(clickedItem);
        visited.Add(clickedItem);

        // BFS 탐색
        //while (toCheck.Count > 0)
        {
            BoardItem currentItem = toCheck.Dequeue();
            Debug.Log("검사할 타겟 아이템 : " + currentItem.index + $" ({itemObjects[currentItem.index]}) ");
            matchingItems.Add(currentItem);

            // 상하좌우 이웃 아이템 검사
            // currentItem index의 왼쪽(-1), 오른쪽(1), 위(-8), 아래(8) 인덱스 검사
            int[] directions = { -1, 1, -8, 8 };
            foreach (int index in directions)
            {
                int neighborIndex = currentItem.index + index;
                if (IsValidIndex(neighborIndex))
                {
                    BoardItem neighbor = items[neighborIndex];
                    Debug.Log("direction : " + neighborIndex + $" ({itemObjects[neighborIndex]}) ");
                    if (clickedItem.spriteRenderer.sprite.name == neighbor.spriteRenderer.sprite.name)
                    {
                        Debug.Log("해당 이웃은 종류가 같음");
                        toCheck.Enqueue(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
        }

        // 최소 3개 이상이어야 매치로 인정
        if (matchingItems.Count < 3)
        {
            matchingItems.Clear();
        }

        return matchingItems;
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < items.Length;
    }
    
    void SwapItems(int indexA, int indexB)
    {
        // 위치 스왑
        GameObject tempObject = itemObjects[indexA];
        itemObjects[indexA].GetComponent<SpriteRenderer>().sprite = items[indexB].spriteRenderer.sprite;    // 게임 속 모습 변경
        itemObjects[indexA] = itemObjects[indexB];
        itemObjects[indexB].GetComponent<SpriteRenderer>().sprite = items[indexA].spriteRenderer.sprite;
        itemObjects[indexB] = tempObject;

        BoardItem temp = items[indexA];
        items[indexA] = items[indexB];
        items[indexB] = temp;
        
        

        Debug.Log("스왑 후 : " + $" ({items[indexA].index}) ({itemObjects[items[indexA].index].GetComponent<SpriteRenderer>().sprite.name}) \n{items[indexB].index} : {itemObjects[items[indexB].index].GetComponent<SpriteRenderer>().sprite.name} ");
        // Swap the positions of the sprite renderers
        //items[indexA].index = indexB;
        //items[indexB].index = indexA;

        // Swap indices in the BoardItems
        //int tempIndex = items[indexA].index;
        //items[indexA].index = items[indexB].index;
        //items[indexB].index = tempIndex;
    }

    void FillEmptySpaces()
    {
        
        // 각 열마다 아래로 이동시켜야 할 아이템 수를 계산
        // 아래서부터 위로 검사
        // 빈 공간을 채우기 위해 아래로 이동
        // 맨 위에 빈 공간 채우기
    }
}
