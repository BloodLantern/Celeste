#pragma once

#include "colliders/collider.hpp"

namespace colliders
{
    class Hitbox : public Collider
    {
    public:
        Hitbox() { mType = ColliderType::Hitbox; }
        Hitbox(const calc::Vector2& position, const calc::Vector2& size);

        void Render(ImColor color) const override;

        bool Collide(const calc::Vector2& point) const override;
        bool Collide(const Hitbox& hitbox) const override;
        bool Collide(const Circle& circle) const override;

        inline float GetLeft() const override { return mPosition.x; }
        inline float GetRight() const override { return mPosition.x + mSize.x; }
        inline float GetTop() const override { return mPosition.y; }
        inline float GetBottom() const override { return mPosition.y + mSize.y; }

        calc::Vector2 GetSize() const { return mSize; }
        void SetSize(const calc::Vector2& size) { mSize = size; }

    private:
        calc::Vector2 mSize;
    };
}
