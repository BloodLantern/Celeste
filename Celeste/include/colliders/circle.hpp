#pragma once

#include "colliders/collider.hpp"

namespace colliders
{
    class Circle : public Collider
    {
    public:
        Circle() { mType = ColliderType::Circle; }
        Circle(const calc::Vector2& position, const float& radius);

        void Render(ImColor color) const override;

        bool Collide(const calc::Vector2& point) const override;
        bool Collide(const Hitbox& hitbox) const override;
        bool Collide(const Circle& circle) const override;

        inline float GetLeft() const override { return mPosition.x; }
        inline float GetRight() const override { return mPosition.x + mRadius; }
        inline float GetTop() const override { return mPosition.y; }
        inline float GetBottom() const override { return mPosition.y + mRadius; }

    private:
        float mRadius = 0.f;
    };
}
