#include "entities/entity.hpp"

#include "globals.hpp"
#include "calc/calc.hpp"

namespace celeste
{
    void Entity::Update(float deltaTime)
    {
        // Movement
        mPosition += mVelocity * deltaTime;

        // Gravity
        if (mApplyGravity)
        {
            if (!mOnGround)
                calc::Approach(mVelocity.y, EntitySpeedVerticalMax, EntityGravityAcceleration * deltaTime);
        }
    }
}
