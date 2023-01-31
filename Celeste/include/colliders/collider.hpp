#pragma once

#include <calc/vector2.hpp>

#include "entities/entity.hpp"
#include "colliders/collide.hpp"

struct ImColor;

namespace colliders
{
    // Colliders forward declarations
    class Hitbox;
    class Circle;

    class Collider
    {
    public:
        Collider() {};
        Collider(const calc::Vector2& position);

        virtual void Render(ImColor color) const = 0;

        bool Collide(const celeste::Entity& entity) const { return Collide(*entity.GetCollider()); };
        bool Collide(const Collider& collider) const;
        virtual bool Collide(const calc::Vector2& point) const = 0;
        virtual bool Collide(const Hitbox& hitbox) const = 0;
        virtual bool Collide(const Circle& circle) const = 0;

        virtual inline float GetLeft() const = 0;
        virtual inline float GetRight() const = 0;
        virtual inline float GetTop() const = 0;
        virtual inline float GetBottom() const = 0;

        calc::Vector2 GetPosition() const { return mPosition; }
        void SetPosition(const calc::Vector2& position) { mPosition = position; }
        ColliderType GetType() const { return mType; }
        void SetType(const ColliderType type) { mType = type; }
        celeste::Entity* GetEntity() const { return mEntity; }
        void SetEntity(celeste::Entity* entity) { mEntity = entity; }

    protected:
        calc::Vector2 mPosition;
        ColliderType mType = ColliderType::None;

        celeste::Entity* mEntity = nullptr;
    };
}
