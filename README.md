# KAIROS-01: Laboratorio Cinético

Prototipo 2D desarrollado en Unity 6.3 LTS.

## Concepto
Kiro es un robot de calibración que debe recorrer una cámara de pruebas, utilizar una caja física como apoyo, recuperar un núcleo cinético y alcanzar la compuerta de salida.

## Controles
- A / D o flechas: movimiento horizontal
- Espacio: salto
- R: reiniciar después de completar la prueba

## Implementación principal
- PlayerController en C#
- Rigidbody2D y Collider2D
- Input.GetAxisRaw para movimiento responsivo
- detección de suelo mediante isGrounded
- plataformas semisólidas
- obstáculo físico empujable
- coleccionable mediante Trigger
- estado final de victoria

## Game Feel
- Velocidad: 4.4
- Fuerza de salto: 11.5
- Mass: 1
- Gravity Scale: 4
- Linear Damping del jugador: 0

## Resolución objetivo
1280 x 720 (16:9)
