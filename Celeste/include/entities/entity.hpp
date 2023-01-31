#pragma once

#include <calc/vector2.hpp>

namespace celeste
{
    constexpr float EntityGravityAcceleration = 900.f;
    constexpr float EntitySpeedVerticalMax = 160.f;

    class Entity
    {
    public:
        virtual void Update(float deltaTime);
        virtual void Draw() = 0;

    protected:
        calc::Vector2 mPosition;
        calc::Vector2 mVelocity;

        bool mOnGround = false;
        bool mApplyGravity = false;
    };
}
