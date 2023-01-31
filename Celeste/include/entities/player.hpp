#pragma once

#include "entity.hpp"

namespace celeste
{
    constexpr float PlayerAccelerationHorizontal = 1000.f;
    constexpr float PlayerSpeedHorizontalMax = 90.f;

    class Player : public Entity
    {
    public:
        Player();

        virtual void Update(float deltaTime) override;
        virtual void Draw() override;
    };
}
