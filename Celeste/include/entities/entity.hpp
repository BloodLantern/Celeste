#pragma once

#include <calc/vector2.hpp>

// Collider forward declaration
namespace colliders { class Collider; }

namespace celeste
{
    constexpr float EntityGravityAcceleration = 900.f;
    constexpr float EntitySpeedVerticalMax = 160.f;

    class Entity
    {
    public:
        virtual void Update(float deltaTime);
        virtual void Draw() = 0;

        calc::Vector2 GetPosition() const { return mPosition; }
        void SetPosition(const calc::Vector2& position) { mPosition = position; }
        calc::Vector2 GetVelocity() const { return mVelocity; }
        void SetVelocity(const calc::Vector2& velocity) { mVelocity = velocity; }
        colliders::Collider* GetCollider() const { return mCollider; }
        void SetCollider(colliders::Collider* collider) { mCollider = collider; }
        bool GetOnGround() const { return mOnGround; }
        void SetOnGround(bool onGround) { mOnGround = onGround; }
        bool GetApplyGravity() const { return mApplyGravity; }
        void SetApplyGravity(bool applyGravity) { mApplyGravity = applyGravity; }

    protected:
        calc::Vector2 mPosition;
        calc::Vector2 mVelocity;

        colliders::Collider* mCollider;

        bool mOnGround = false;
        bool mApplyGravity = false;
    };
}
