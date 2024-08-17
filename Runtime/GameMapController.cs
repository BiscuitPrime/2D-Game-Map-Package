using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// This script handles the movement behaviour of game objects moved through a 2D space by the player's mouse.
/// <para></para>
/// Component based : should be put on the objects that need to be moved.
/// </summary>
namespace BiscuitPrime.GameMap2D
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class GameMapController : MonoBehaviour
    {
        [Header("Internal References")]
        private SpriteRenderer _mapSpriteRenderer;
        private BoxCollider2D _boxCollider;

        [SerializeField, Tooltip("Will restrict the map to the screen, avoiding pushing it outside of its edge.")] private bool _isMoveRestrictedToScreen = true; //whether or not you wish to limit the object's movements to the screen

        [Header("Movement variables")]
        private Transform _draggedHitTransform = null;  //RaycastHit of the current object being selected/dragged by the player
        private Vector3 _offset;                        //offset between mouse position and dragged object's center
        private float _xBound, _yBound;                 //limits of the selected object, used to clamp its movements in the game screen

        private void OnValidate()
        {
            _mapSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            Assert.IsNotNull(_mapSpriteRenderer,"[GameMapMovementController] : The Game Map does not possess a child with SpriteRenderer.");
            _boxCollider = GetComponent<BoxCollider2D>();
            if(transform.localScale != Vector3.one)
            {
                Debug.LogWarning("[GameMovementController] : To ensure the correct behaviour from the game map, "+this.gameObject.name+"'s scale must remain at one. \nYou can change the child's scale freely however.");
                transform.localScale = Vector3.one;
            }
        }

        private void Update()
        {
            if (_draggedHitTransform != null)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + _offset; //we obtain the new position of the object at the player's request
                if (_isMoveRestrictedToScreen)
                {
                    //we limit the object's movements to the object's bounds : 
                    pos.x = Mathf.Clamp(pos.x, -_xBound, _xBound);
                    pos.y = Mathf.Clamp(pos.y, -_yBound, _yBound);
                }
                //we set the target objects position.
                transform.position = pos;
            }
        }

        /// <summary>
        /// Function called when the mouse clicks on this object.
        /// Will create the Raycast hit that will indicate the mouse's hit position.
        /// </summary>
        private void OnMouseDown()
        {
            // Cast our own ray.
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, float.PositiveInfinity);
            if (hit)
            {
                // If we hit, record the transform of the object we hit.
                _draggedHitTransform = hit.transform;
                //We obtain the selected object's bounds :
                ObtainBounds();
                // And record the offset.
                _offset = _draggedHitTransform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        /// <summary>
        /// Function called when the player stops clicking.
        /// This means the player stops dragging the map, so we relinquish the _draggedHitTransform.
        /// </summary>
        private void OnMouseUp()
        {
            _draggedHitTransform = null;
        }

        /// <summary>
        /// Function that will obtain the bound of the map's sprite in world coordinates.
        /// </summary>
        private void ObtainBounds()
        {
            Vector3 ex = _mapSpriteRenderer.sprite.bounds.extents;
            ex.x = ex.x * _mapSpriteRenderer.transform.localScale.x;
            ex.y = ex.y * _mapSpriteRenderer.transform.localScale.y;
            Vector3 topRight = Camera.main.ViewportToWorldPoint(Vector3.one);
            _xBound = Mathf.Abs(topRight.x - ex.x);
            _yBound = Mathf.Abs(topRight.y - ex.y);
            AdjustCollider();
        }

        /// <summary>
        /// This function will automatically adjust the size of the box collider as to make it fit with the map's sprite.
        /// </summary>
        public void AdjustCollider()
        {
            _boxCollider.size = new Vector3(_mapSpriteRenderer.sprite.bounds.size.x * _mapSpriteRenderer.transform.localScale.x, 
                                                            _mapSpriteRenderer.sprite.bounds.size.y * _mapSpriteRenderer.transform.localScale.y);
        }

        /// <summary>
        /// This function will give the user the option to adjust the collider from the menu itself, without launching the game.
        /// </summary>
        [MenuItem("GameMap/AdjustCollider")]
        static private void AdjustColliderMenu()
        {
            foreach(var a in Selection.gameObjects)
            {
                if(a is GameObject && a.GetComponent<GameMapController>() != null)
                {
                    a.GetComponent<GameMapController>().AdjustCollider();
                }
            }
        }
    }
}