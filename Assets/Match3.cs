using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Match3 : MonoBehaviour, IPointerClickHandler
{
    public GameObject board;
    public GameObject invisibleBoard;
    public GameObject clickedBoard;
    public GameObject effectBoard;
    public Canvas canvas;   // board가 위치한 캔버스 (this)
    public Sprite[] itemPrefab;
    public GameObject imagePrefab;
    public GameObject effectPrefab;
    public float swapDuration = 0.3f;
    public float effectDuration = 1f;
    
    // TODO : 콤보시스템 클래스 구현하기
    // 프로퍼티로 10번마다 스페셜 프로젝타일 쏘는거 가능할듯?
    //public int combo = 0;
    
    private bool isClickingItem = false;
    private bool isSwapping = false;
    private GameObject firstClickedItem;
    private List<GameObject> boardGrid = new List<GameObject>();        // 백그라운드 오브젝트
    private List<Image> invisibleBoardGrid = new List<Image>();    // 백그라운드 오브젝트
    private GameObject[] clickedBoardGrid = new GameObject[40];      // 이미지 오브젝트
    private GameObject[] effectBoardGrid = new GameObject[40];      // 이펙트 오브젝트
    private string number;

    public static Match3 Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetBoard();

        int i = 0;
        foreach (var image in clickedBoard.GetComponentsInChildren<Image>(true))
        {
            clickedBoardGrid[i++] = image.gameObject;
        }

        i = 0;
        foreach (var effect in effectBoard.GetComponentsInChildren<RectTransform>())
        {
            if(effect != effectBoard.transform)
                effectBoardGrid[i++] = effect.gameObject;
        }
    }

    private void SetBoard()
    {
        foreach (RectTransform background in board.GetComponentsInChildren<RectTransform>())
        {
            // Board 자신은 제외하고 직접 자식들만 리스트에 추가
            if (background != board.transform && background.gameObject.CompareTag("BoardCell")) 
            {
                boardGrid.Add(background.gameObject);
                
                // TODO : 페이즈가 지날수록 or 시간이 지날수록 아이템 종류 개수가 늘어나도록 range 범위 조정
                SetSprite(background.GetComponentsInChildren<Image>()[1], itemPrefab[Random.Range(0, itemPrefab.Length)]);
            }
        }
        
        foreach (Image background in invisibleBoard.GetComponentsInChildren<Image>())
        {
            // Board 자신은 제외하고 직접 자식들만 리스트에 추가
            if (background != invisibleBoard.transform && background.gameObject.CompareTag("BoardCell")) 
            {
                invisibleBoardGrid.Add(background);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isSwapping) return;
        
        // 첫번째 클릭
        if (!isClickingItem)
        {
            firstClickedItem = GetSlotUnderPointer(eventData);  // 이미지 오브젝트
            
            // 아이템이 있는 곳을 클릭했을 때
            if (firstClickedItem != null)
            {
                isClickingItem = true;
                
                // 클릭 UI effect
                number = ExtractNumber(firstClickedItem.transform.parent.name);
                clickedBoardGrid[int.Parse(number)].SetActive(true);
                AudioManager.Instance.EffectPlay("Click");                
            }
        }
        // 두번째 클릭
        else
        {
            isSwapping = true; // Disable further clicks
            clickedBoardGrid[int.Parse(number)].SetActive(false);
            GameObject secondClickedItem = GetSlotUnderPointer(eventData);
            
            if (secondClickedItem != null && secondClickedItem != firstClickedItem)
            {
                AudioManager.Instance.EffectPlay("Click"); 
                
                // 두 아이템 교환 
                Swap(firstClickedItem, secondClickedItem);
            }
            else
            {
                
                Debug.LogWarning("두번째 아이템 선택이 잘못되었습니다.");
                isClickingItem = false;
                isSwapping = false; // Re-enable clicks
            }
        }
    }

    private void Swap(GameObject first, GameObject second)
    {
        StartCoroutine(SwapCoroutine(first, second));
    }
    
    private IEnumerator SwapCoroutine(GameObject item1, GameObject item2)
    {
        // Item1과 Item2의 부모 Background를 스왑하기 전 위치를 저장
        Transform parent1 = item1.transform.parent;
        Transform parent2 = item2.transform.parent;
        
        // 옮기기 전 부모를 Canvas로 옮김
        item1.transform.SetParent(canvas.transform, true);
        item2.transform.SetParent(canvas.transform, true);
        
        // 부모 옮기고 나서의 시작 위치 
        Vector3 startPosition1 = item1.GetComponent<RectTransform>().anchoredPosition;
        Vector3 startPosition2 = item2.GetComponent<RectTransform>().anchoredPosition;

        // Start the animation over swapDuration
        float elapsedTime = 0f;
        while (elapsedTime < swapDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / swapDuration);

            // Smoothly interpolate positions
            item1.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(startPosition1, startPosition2, t);
            item2.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(startPosition2, startPosition1, t);

            yield return null;
        }

        // Set items to new parents
        item1.transform.SetParent(parent2, false);
        item2.transform.SetParent(parent1, false);

        // 부모 바꾼 후 위치 초기화
        item1.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        item2.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        //Debug.Log("Items have been swapped visually.");
        
        List<GameObject> matchingItemsA = FindMatchingItems(item2);
        List<GameObject> matchingItemsB = FindMatchingItems(item1);
        
        // 매칭된거 없으면 다시 스왑 
        if (isClickingItem && matchingItemsA.Count + matchingItemsB.Count == 0)
        {
            ComboManager.Instance.ResetComboCount();  // 콤보 초기화

            Debug.Log("매칭된 것이 없으므로 다시 원위치");
            yield return StartCoroutine(RevertSwapCoroutine(item1, item2));
        }
        else
        {
            ComboManager.Instance.IncreaseComboCount();
            // TODO : 콤보 숫자 UI 뜨게하기 

            // 3 매치된 게 있으면 빈 이미지로 바꿈 
            yield return new WaitForSeconds(0.1f);
            DestroyItems(matchingItemsA);
            DestroyItems(matchingItemsB);
            
            // 이펙트 나오게 한 후 이펙트 없어지면
            ShowEffect(matchingItemsA);
            ShowEffect(matchingItemsB);
            AudioManager.Instance.EffectPlay("Pop");
            yield return new WaitForSeconds(effectDuration);
            
            // 먼저 있는 아이템들을 내리고
            ///Debug.Log("-------------------보드에 있는 아이템 내리기------------------");
            yield return FillItems();
            // 위에서 fill 채우고 새로 채워진 칸의 열, 행 검사해서 3매치 있는지 체크 

            //Debug.Log("-------------------전체 행, 열 중 3매치 찾기------------------");
            List<GameObject> allMatchingItems = FindAllMatchingItems();

            while (allMatchingItems.Count > 0)
            {
                DestroyItems(allMatchingItems);
                ShowEffect(allMatchingItems);
                AudioManager.Instance.EffectPlay("Pop");
                yield return new WaitForSeconds(effectDuration);
                
                yield return FillItems();

                allMatchingItems = FindAllMatchingItems();
            }
            
            // FindAllMatchingItems의 리턴 리스트 길이가 0일 때 IsMatchPossible 호출
            //Debug.Log("------------------3매치 가능성이 있는지 전체 행, 열 검사-------------------");

            while (!IsMatchPossible())
            {
                yield return new WaitForSeconds(0.5f);
                // 보드 재생성 이펙트 on
                UIManager.Instance.ActivateRefreshImage();
                
                // 보드 재생성
                foreach (GameObject background in boardGrid)
                {
                    SetSprite(background.GetComponentsInChildren<Image>()[1], itemPrefab[Random.Range(0, itemPrefab.Length)]);
                }
                yield return new WaitForSeconds(0.5f);
                //Debug.Log("보드 재생성 완료");
                
                // 보드 재생성 이펙트 off, 캔버스 보이게
                UIManager.Instance.DeactivateRefreshImage();
                
                yield return null;
            }
        }

        isSwapping = false; // Re-enable clicks
        isClickingItem = false;
    }
    
    private IEnumerator RevertSwapCoroutine(GameObject item1, GameObject item2)
    {
        //Debug.Log($"{item1.transform.parent.gameObject.name} / {item1.transform.parent.parent.gameObject.name} <-> {item2.transform.parent.gameObject.name} / {item2.transform.parent.parent.gameObject.name}");
        
        // Item1과 Item2의 부모 Background를 스왑하기 전 위치를 저장
        Transform parent1 = item1.transform.parent;
        Transform parent2 = item2.transform.parent;
        
        // 옮기기 전 부모를 Canvas로 옮김
        item1.transform.SetParent(canvas.transform, true);
        item2.transform.SetParent(canvas.transform, true);
        //item1.transform.parent = canvas.transform;
        //item2.transform.parent = canvas.transform;
        
        // 부모 옮기고 나서의 시작 위치 
        Vector3 startPosition1 = item1.GetComponent<RectTransform>().anchoredPosition;
        Vector3 startPosition2 = item2.GetComponent<RectTransform>().anchoredPosition;

        GameObject placeholder1 = Instantiate(imagePrefab);
        GameObject placeholder2 = Instantiate(imagePrefab);

        placeholder1.transform.SetParent(parent1, false);
        placeholder2.transform.SetParent(parent2, false);

        SetSprite(placeholder1.GetComponent<Image>(), null);
        SetSprite(placeholder2.GetComponent<Image>(), null);

        // Position placeholders correctly
        placeholder1.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        placeholder2.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // Start the animation over swapDuration
        float elapsedTime = 0f;
        while (elapsedTime < swapDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / swapDuration);

            // Smoothly interpolate positions
            item1.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(startPosition1, startPosition2, t);
            item2.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(startPosition2, startPosition1, t);

            yield return null;
        }

        // Set items to new parents
        /*item1.transform.SetParent(parent2, false);
        item2.transform.SetParent(parent1, false);

        // 부모 바꾼 후 위치 초기화
        item1.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        item2.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);*/

        SetSprite(placeholder1.GetComponent<Image>(), item2.GetComponent<Image>().sprite);
        SetSprite(placeholder2.GetComponent<Image>(), item1.GetComponent<Image>().sprite);

        // Destroy original image objects
        Destroy(item1);
        Destroy(item2);
    }
    
    private GameObject GetSlotUnderPointer(PointerEventData eventData)
    {
        // TODO : background 자식인 item에 item 스크립트 넣고 그걸로 구별하기 
        GameObject item = eventData.pointerCurrentRaycast.gameObject;
        if (item != null)
        {
            if (item.CompareTag("Item"))
                return item;
        }
        Debug.Log("잘못 클릭");
        return null;
    }

    private List<GameObject> FindMatchingItems(GameObject clickedItem)  // clickedItem은 이미지 오브젝트 
    {
        List<GameObject> matchingItems = new List<GameObject>();

        // item 부모의 인덱스 얻기
        int indexOfClickedItem = GetIndexOfBoardGrid(clickedItem);
        if (indexOfClickedItem == -1)
        {
            Debug.Log("클릭한 아이템의 부모가 없습니다. 잘못 클릭");
            return new List<GameObject>();
        }
        
        // 상하좌우 이웃 아이템 검사
        // currentItem index의 왼쪽(-1), 오른쪽(1), 위(-8), 아래(8) 인덱스 검사
        string clickedItemSprite = clickedItem.GetComponent<Image>().sprite.name;       // 백그라운드 오브젝트
        
        // Find horizontal matches
        List<GameObject> horizontalMatches = FindLineMatches(indexOfClickedItem, clickedItemSprite, new int[] { -1, 1 }, true);
        // Find vertical matches
        List<GameObject> verticalMatches = FindLineMatches(indexOfClickedItem, clickedItemSprite, new int[] { -8, 8 }, false);

        // Add matches to the final list if they have 3 or more items
        if (horizontalMatches.Count >= 3)
        {
            //Debug.Log("수평선 결과 : " + horizontalMatches.Count);
            matchingItems.AddRange(horizontalMatches);
        }
        if (verticalMatches.Count >= 3)
        {
            //Debug.Log("수직선 결과 : " + verticalMatches.Count);
            matchingItems.AddRange(verticalMatches);
        }

        //Debug.Log("매칭 결과 : " + matchingItems.Count);
        return matchingItems;
    }
    
    private List<GameObject> FindAllMatchingItems()
    {
        List<GameObject> allMatchingItems = new List<GameObject>();

        // Iterate through each row
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int index = row * 8 + col;
                Image imageObject = boardGrid[index].GetComponentsInChildren<Image>()[1];

                // Check for horizontal matches
                List<GameObject> horizontalMatches = FindLineMatches(index, imageObject.sprite.name, new int[] { -1, 1 }, true);
                if (horizontalMatches.Count >= 3)
                {
                    allMatchingItems.AddRange(horizontalMatches);
                }

                // Check for vertical matches
                List<GameObject> verticalMatches = FindLineMatches(index, imageObject.sprite.name, new int[] { -8, 8 }, false);
                if (verticalMatches.Count >= 3)
                {
                    allMatchingItems.AddRange(verticalMatches);
                }
            }
        }

        // Remove duplicates from the matching items list
        allMatchingItems = allMatchingItems.Distinct().ToList();

        string str = "";
        foreach (var go in allMatchingItems)
        {
            str += go.transform.parent.name + ", ";
        }
        //Debug.Log("전체 행, 열 3매치된 아이템 인덱스 위치 : " + str);

        return allMatchingItems;
    }
    
    private List<GameObject> FindLineMatches(int indexOfClickedItem, string spriteName, int[] directions, bool isHorizontal)
    {
        List<GameObject> lineMatches = new List<GameObject>();
        HashSet<GameObject> visited = new HashSet<GameObject>();
        lineMatches.Add(boardGrid[indexOfClickedItem].GetComponentsInChildren<Image>()[1].gameObject); // Add the starting item itself
        visited.Add(boardGrid[indexOfClickedItem].GetComponentsInChildren<Image>()[1].gameObject);

        foreach (int index in directions)
        {
            int neighborIndex = indexOfClickedItem + index;

            // Check in one direction until no match is found
            while (IsValidIndex(neighborIndex, index, isHorizontal))
            {
                GameObject neighbor = boardGrid[neighborIndex].GetComponentsInChildren<Image>()[1].gameObject;
                //Debug.Log($"{index} / {neighborIndex} / {boardGrid[neighborIndex].name}");
                if (neighbor.GetComponent<Image>()?.sprite.name == spriteName && !visited.Contains(neighbor))
                {
                    lineMatches.Add(boardGrid[neighborIndex].GetComponentsInChildren<Image>()[1].gameObject);
                    //Debug.Log("해당 이웃은 종류가 같음");
                    neighborIndex += index;
                }
                else
                {
                    // 다르면 바로 끝 
                    break;
                }
            }
        }

        return lineMatches; // 이미지 오브젝트 
    }
    
    private bool IsMatchPossible()
    {
        // Iterate through each cell in the board
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                int index = row * 8 + col;
                Image imageObject = boardGrid[index].GetComponentsInChildren<Image>()[1];
                string currentSpriteName = imageObject.sprite.name;

                // Check for horizontal 2-match
                if (col < 7 && boardGrid[index + 1].GetComponentsInChildren<Image>()[1].sprite.name == currentSpriteName)
                {
                    // Check surroundings for a potential 3rd match item
                    if (HasSurroundingMatch(index, index + 1, currentSpriteName, true))
                    {
                        Debug.Log($"horizontal : {true} / index1 : {index} / index2 : {index+1}");
                        return true;
                    }
                }

                // Check for vertical 2-match
                if (row < 4 && boardGrid[index + 8].GetComponentsInChildren<Image>()[1].sprite.name == currentSpriteName)
                {
                    // Check surroundings for a potential 3rd match item
                    if (HasSurroundingMatch(index, index + 8, currentSpriteName, false))
                    {
                        Debug.Log($"horizontal : {false} / index1 : {index} / index2 : {index + 8}");
                        return true;
                    }
                }
                
                // 1 - 0 - 1 매치
                if (col < 6 && boardGrid[index + 2].GetComponentsInChildren<Image>()[1].sprite.name == currentSpriteName)
                {
                    if (HasSurroundingMatch2(index, currentSpriteName, true))
                    {
                        Debug.Log($"1 - 0 - 1 horizontal : {true} / index1 : {index} / index2 : {index + 2}");
                        return true;
                    }
                }
                
                if (row < 3 && boardGrid[index + 2 * 8].GetComponentsInChildren<Image>()[1].sprite.name == currentSpriteName)
                {
                    if (HasSurroundingMatch2(index, currentSpriteName, false))
                    {
                        Debug.Log($"1 - 0 - 1 horizontal : {false} / index1 : {index} / index2 : {index + 2}");
                        return true;
                    }
                }
            }
        }

        // No possible matches found
        return false;
    }

    // ■ ■ □
    // index1 < index2
    private bool HasSurroundingMatch(int index1, int index2, string spriteName, bool isHorizontal)
    {
        // Check horizontal surroundings
        if (isHorizontal)
        {
            // Check left of index1
            if (index1 % 8 > 1 && boardGrid[index1 - 2].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }

            // Check right of index2
            if (index2 % 8 < 6 && boardGrid[index2 + 2].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
            
            // Check above or below index1
            if (index1 % 8 > 0 && index1 % 8 < 7 && index1 / 8 > 0 && boardGrid[index1 - 1 - 8].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
            if (index1 % 8 > 0 && index1 % 8 < 7 && index1 / 8 < 4 && boardGrid[index1 - 1 + 8].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
            
            // Check above or below index2
            if (index2 % 8 > 0 && index2 % 8 < 7 && index2 / 8 > 0 && boardGrid[index2 + 1 - 8].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
            if (index2 % 8 > 0 && index2 % 8 < 7 && index2 / 8 < 4 && boardGrid[index2 + 1 + 8].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
        }
        else // Check vertical surroundings
        {
            // Check above index1
            if (index1 / 8 > 1 && boardGrid[index1 - 8 * 2].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }

            // Check below index2
            if (index2 / 8 < 3 && boardGrid[index2 + 8 * 2].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
            
            if (index1 % 8 > 0 && index1 % 8 < 7 && index1 / 8 > 0 && boardGrid[index1 - 1 - 8].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
            if (index1 % 8 > 0 && index1 % 8 < 7 && index1 / 8 > 0 && boardGrid[index1 + 1 - 8].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
            
            // Check above or below index2
            if (index2 % 8 > 0 && index2 % 8 < 7 && index2 / 8 < 4 && boardGrid[index2 - 1 + 8].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
            if (index2 % 8 > 0 && index2 % 8 < 7 && index2 / 8 < 4 && boardGrid[index2 + 1 + 8].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
        }
        return false;
    }

    // ■ □ ■
    // index1 ■ + 1 = index2 □
    private bool HasSurroundingMatch2(int index1, string spriteName, bool isHorizontal)
    {
        if (isHorizontal)
        {
            int index2 = index1 + 1;
            if (index2 / 8 > 0 && boardGrid[index2 - 8].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
            if (index2 / 8 < 4 && boardGrid[index2 + 8].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
        }
        else
        {
            int index2 = index1 + 8;
            if (index2 % 8 > 0 && boardGrid[index2 - 1].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
            if (index2 % 8 < 7 && boardGrid[index2 + 1].GetComponentsInChildren<Image>()[1].sprite.name == spriteName)
            {
                return true;
            }
        }
        return false;
    }

    private void DestroyItems(List<GameObject> matchingItems)
    {
        foreach (var item in matchingItems)
        {
            SetSprite(item.GetComponent<Image>(), null);    // 이미지 오브젝트
        }
    }

    private void ShowEffect(List<GameObject> matchingItems)
    {
        foreach (var imageObject in matchingItems)
        {
            int index = GetIndexOfBoardGrid(imageObject);
            GameObject go = Instantiate(effectPrefab, effectBoardGrid[index].transform);
            Destroy(go, 2f);
        }
    }

    IEnumerator FillItems()
    {
        // 전체 열을 돌면서 각 열마다 sprite가 null인 칸의 개수가 몇개인지 체크 
        List<List<int>> nullCount = CheckBlankOfColumn();
        List<Coroutine> coroutines = new List<Coroutine>();
        for (int i = 0; i < nullCount.Count; i++)
        {
            // 보드에서 아이템 내리기
            if (nullCount[i].Count() > 0)
            {
                string str = "";
                foreach (var value in nullCount[i])
                {
                    str += value + ", ";
                }
                //Debug.Log($"열 {i}에서 비어있는 칸의 개수 : {nullCount[i].Count} / 비어있는 row index : {str}");
                coroutines.Add(StartCoroutine(DownItem(i, nullCount[i])));
            }
        }
        foreach (Coroutine coroutine in coroutines)
        {
            yield return coroutine;
        }
        
        coroutines.Clear();
        //Debug.Log("-----------아이템 새로 생성해서 내리기-----------");
        // 열마다 null 개수 확인
        List<List<int>> nullCountForInvisible = CheckBlankOfColumn();
        
        // invisible 보드에서 아이템 생성해서 내리기
        for (int i = 0; i < nullCountForInvisible.Count; i++)
        {
            // 보드에서 아이템 내리기
            if(nullCountForInvisible[i].Count() > 0)
                coroutines.Add(StartCoroutine(MakeAndDownItem(i, nullCountForInvisible[i].Count())));
        }
        
        foreach (Coroutine coroutine in coroutines)
        {
            yield return coroutine;
        }
    }

    IEnumerator DownItem(int col, List<int> nullRowIndex)
    {
        List<Coroutine> coroutines = new List<Coroutine>();
        // 보드에서 null로 옮길 수 있는 아템 개수 : nullRowIndex[0]
        for (int i = 0; i < nullRowIndex[0]; i++)
        {
            //Debug.Log($"row{nullRowIndex[0] - 1 - i} -> row{nullRowIndex[nullRowIndex.Count - 1] - i}로 이동 완료");
            GameObject startBoardObject = boardGrid[col + 8 * (nullRowIndex[0] - 1 - i)].GetComponentsInChildren<Image>()[1].gameObject;
            GameObject targetBoardObject = boardGrid[col + 8 * (nullRowIndex[nullRowIndex.Count - 1] - i)].GetComponentsInChildren<Image>()[1].gameObject; // null 이미지 오브젝트
            //Color transparent = targetBoardObject.GetComponent<Image>().color;
            //transparent.a = 0f;
            //targetBoardObject.GetComponent<Image>().color = transparent;

            coroutines.Add(StartCoroutine(RevertSwapCoroutine(startBoardObject, targetBoardObject)));
            
            // TODO : 모든 null 칸의 투명도 0으로 만들었다가 sprite 설정되면 255로 바꾸기 (property 이용해서)
            //Color color = startBoardObject.GetComponent<Image>().color;
            //color.a = 255f;
            //targetBoardObject.GetComponent<Image>().color = color;
        }
        
        foreach (Coroutine coroutine in coroutines)
        {
            yield return coroutine;
        }
    }

    IEnumerator MakeAndDownItem(int col, int count)
    {
        // invisible 보드에 차례대로 만들기 
        for (int i = 0; i < count; i++)
        {
            SetSprite(invisibleBoardGrid[col + 8 * i].GetComponentsInChildren<Image>()[1], itemPrefab[Random.Range(0, itemPrefab.Length)]);
        }
        
        List<Coroutine> coroutines = new List<Coroutine>();
        for (int i = 0; i < count; i++)
        {
            GameObject startObject = invisibleBoardGrid[col + 8 * i].GetComponentsInChildren<Image>()[1].gameObject;
            GameObject targetObject = boardGrid[col + 8 * (count - 1 - i)].GetComponentsInChildren<Image>()[1].gameObject; // null 이미지 오브젝트
            coroutines.Add(StartCoroutine(RevertSwapCoroutine(startObject, targetObject)));
            
            //Debug.Log($"invisible index{col + 8 * i} -> index{col + 8 * (count - 1 - i)}로 이동 완료/col {col}/count {count}");
        }
        
        // waiting for all coroutines to finish
        foreach (Coroutine coroutine in coroutines)
        {
            yield return coroutine;
        }
    }

    // TODO : board 세팅할 때 이미지 배열들도 만들어서 그 배열들로 체크하기 
    private List<List<int>> CheckBlankOfColumn()
    {
        List<List<int>> returnList = new List<List<int>>();
        int colNum = 8;     // 총 열의 개수 
        int rowNum = 5;
        for (int i = 0; i < colNum; i++)
        {
            int sum = 0;
            List<int> nullIndex = new List<int>();
            for (int j = 0; j < rowNum; j++)
            {
                if (boardGrid[i + colNum * j].GetComponentsInChildren<Image>()[1].sprite == null)
                {
                    sum++;
                    nullIndex.Add(j);
                }
            }
            returnList.Add(nullIndex);
        }

        return returnList;
    }

    private int GetIndexOfBoardGrid(GameObject clickedItem)
    {
        GameObject parent = clickedItem.transform.parent.gameObject;
        if (boardGrid.Contains(parent))
            return boardGrid.IndexOf(parent);
        return -1;
    }

    // 왼 오른일 때는 다른 행이면 검사하면 X
    private bool IsValidIndex(int index, int offset, bool isHorizontal) // index = 타겟 인덱스 + 오프셋 
    {
        // 인덱스가 유효한지 확인
        int neighborIndex = index + offset;
        if (index < 0 || index >= boardGrid.Count)
            return false;
        
        // 같은 행에 있는지 확인
        if (isHorizontal)
        {
            int rowLength = 8;  // 한 줄에 있는 아이템 수
            int currentRow = (index - offset) / rowLength;
            int targetRow = index / rowLength;
            return currentRow == targetRow;
        }
        return true;
    }

    private void SetSprite(Image image, Sprite sprite)
    {
        image.sprite = sprite;

        // Update the transparency based on the sprite being null or not
        Color color = image.color;
        color.a = (sprite == null) ? 0f : 255f; // Set alpha to 0 if sprite is null, otherwise set it to 1
        image.color = color;
    }
    
    private static string ExtractNumber(string input)
    {
        // Define a regular expression to match numbers within parentheses
        Match match = Regex.Match(input, @"\((\d+)\)");
        
        if (match.Success)
        {
            // If a match is found, return the captured group
            return match.Groups[1].Value;
        }

        // Return an empty string or handle the error if no match is found
        return string.Empty;
    }
}
