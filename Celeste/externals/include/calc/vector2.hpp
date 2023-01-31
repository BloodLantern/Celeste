#pragma once

#include <iostream>
#include <compare>

namespace calc
{
#pragma region Vector2
	/// @brief The Vector2 class represents either a two-dimensional vector or a point.
	class Vector2
	{
	public:
		float x, y;

		/// @brief A Vector2 with both its components equal to 0.
		static const Vector2 Zero;
		/// @brief A Vector2 with { x = 0, y = 1 }, used to represent a unit on the Y axis.
		static const Vector2 UnitY;
		/// @brief A Vector2 with { x = 1, y = 0 }, used to represent a unit on the X axis.
		static const Vector2 UnitX;

		Vector2();
		/// @brief Constructs a Vector2 with both its components set to 'xy'.
		Vector2(const float xy);
		Vector2(const float x, const float y);
		Vector2(const Vector2& other);
		/// @brief Constructs a Vector2 from 'p1' to 'p2'
		Vector2(const Vector2& p1, const Vector2& p2);
		~Vector2() {}

		/// @brief Returns the length of the vector.
		float GetNorm() const;
		/// @brief Returns the squared length of the vector.
		float GetSquaredNorm() const;
		/// @brief Normalizes the vector.
		/// @return A vector with the same direction but a length of one.
		Vector2 Normalize() const;
		/// @brief Returns the normal vector to this one.
		/// @return A vector with the same length but a normal direction.
		Vector2 GetNormal() const;
		/// @brief Returns the angle between the beginning and the end of this vector.
		/// @return An angle in radians.
		float Angle() const;
		/// @brief Rotates the vector by the specified angle.
		/// @param angle The angle in radians.
		Vector2 Rotate(float angle) const;

		friend auto operator<=>(const Vector2& a, const Vector2& b) = default;
	};

	/// @brief Returns the angle between 'a' and 'b'.
	float Angle(const Vector2 a, const Vector2 b);
	/// @brief Returns the dot product of 'a' and 'b'.
	float DotProduct(const Vector2 a, const Vector2 b);
	/// @brief Returns the determinant of 'a' and 'b'.
	float Determinant(const Vector2 a, const Vector2 b);

	Vector2 operator+(const Vector2 a, const Vector2 b);
	Vector2 operator-(const Vector2 a, const Vector2 b);
	Vector2 operator-(const Vector2 a);
	Vector2 operator*(const Vector2 a, const Vector2 b);
	Vector2 operator*(const Vector2 v, const float factor);
	Vector2 operator/(const Vector2 a, const Vector2 b);
	Vector2 operator/(const Vector2 v, const float factor);

	Vector2& operator+=(Vector2& a, const Vector2 b);
	Vector2& operator+=(Vector2& v, const float factor);
	Vector2& operator-=(Vector2& a, const Vector2 b);
	Vector2& operator-=(Vector2& v, const float factor);
	Vector2& operator*=(Vector2& a, const Vector2 b);
	Vector2& operator*=(Vector2& v, const float factor);
	Vector2& operator/=(Vector2& a, const Vector2 b);
	Vector2& operator/=(Vector2& v, const float factor);

	std::ostream& operator<<(std::ostream& out, const Vector2& v);
#pragma endregion
	
#pragma region Point2
	/// @brief Returns the distance between 'p1' and 'p2'.
	extern inline float Dist(const Vector2 p1, const Vector2 p2);
	/// @brief Returns the squared distance between 'p1' and 'p2'.
	extern inline float DistSquared(const Vector2 p1, const Vector2 p2);
	/// @brief Returns a Vector2 in between if 'p1' and 'p2'.
	extern inline Vector2 Mid(const Vector2 p1, const Vector2 p2);
	/// @brief Rotates 'p' of 'angle' radians around the origin.
	extern inline Vector2 Rotate(const Vector2 p, const float angle);
	/// @brief Rotates 'p' of 'angle' radians around 'center'.
	extern inline Vector2 Rotate(const Vector2 p, const Vector2 center, float angle);
	/// @brief Rotates 'p' using already-computed cosine and sine around the origin.
	extern inline Vector2 Rotate(const Vector2 p, float cos, float sin);
	/// @brief Rotates 'p' using already-computed cosine and sine around 'center'.
	extern inline Vector2 Rotate(const Vector2 p, const Vector2 center, float cos, float sin);
	/// @brief Scales 'p' 'scale' times around the origin.
	extern inline Vector2 Scale(const Vector2 p, float scale);
	/// @brief Scales 'p' 'scale' times around 'center'.
	extern inline Vector2 Scale(const Vector2 p, const Vector2 center, float scale);
	extern inline Vector2 Circumcenter(const Vector2 p1, const Vector2 p2, const Vector2 p3);
	extern inline Vector2 IsobarycenterTriangle(const Vector2 p1, const Vector2 p2, const Vector2 p3);
#pragma endregion
}
