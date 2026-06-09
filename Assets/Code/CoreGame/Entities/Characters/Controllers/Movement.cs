using Core.Data;
using Core.GameLoop;
using UnityEngine;

namespace CoreGame.Entities.Characters.Controllers
{
    public class Movement : ICharacterComponent, IFixedUpdateListener
    {
        public bool IsInitialized { get; set; }
        public string RuntimeListenerName => "Movement";
        public Condition Condition { get; } = new();
      
        private readonly Rigidbody2D _body;
        private readonly ReactiveProperty<Vector2> _direction;
        private readonly float _speed;

        public Movement(Rigidbody2D rigidbody2D, ReactiveProperty<Vector2> direction, float speed)
        {
            _body = rigidbody2D;
            _direction = direction;
            _speed = speed;
        }
        
        public void GameFixedUpdate(float fixedDeltaTime)
        {
            if (_body == null || !Condition.AreMet())
            {
                return;
            }

            _body.velocity = _direction.Value.normalized * _speed;
        }
    }
}