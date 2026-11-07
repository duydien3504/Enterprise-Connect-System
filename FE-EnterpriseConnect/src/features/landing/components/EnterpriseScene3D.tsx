import React, { useEffect, useRef } from 'react';
import * as THREE from 'three';

interface EnterpriseScene3DProps {
  onSelectAction: () => void;
}

export const EnterpriseScene3D: React.FC<EnterpriseScene3DProps> = ({ onSelectAction }) => {
  const mountRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const container = mountRef.current;
    if (!container) return;

    // 1. Scene, Camera, Renderer Setup
    const scene = new THREE.Scene();

    const camera = new THREE.PerspectiveCamera(
      42,
      container.clientWidth / container.clientHeight,
      0.1,
      1000
    );
    camera.position.set(0, 2.2, 11);

    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    renderer.setSize(container.clientWidth, container.clientHeight);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.25;
    container.appendChild(renderer.domElement);

    // 2. Lighting Rig
    const ambientLight = new THREE.AmbientLight(0xdbeafe, 1.6);
    scene.add(ambientLight);

    const sunLight = new THREE.DirectionalLight(0xffffff, 3.2);
    sunLight.position.set(12, 18, 12);
    scene.add(sunLight);

    const rimLight = new THREE.DirectionalLight(0x38bdf8, 2.0);
    rimLight.position.set(-12, -4, -8);
    scene.add(rimLight);

    const corePointLight = new THREE.PointLight(0x0ea5e9, 4.0, 16);
    corePointLight.position.set(0, 0.5, 0);
    scene.add(corePointLight);

    // 3. Multi-tiered Circular Podium Base
    const podiumGroup = new THREE.Group();
    podiumGroup.position.set(0, -2.4, 0);
    scene.add(podiumGroup);

    // Lower Tier Cylinder
    const lowerGeo = new THREE.CylinderGeometry(4.4, 4.8, 0.45, 64);
    const brushedMetalMat = new THREE.MeshStandardMaterial({
      color: 0x1e3a8a,
      metalness: 0.85,
      roughness: 0.25,
    });
    const lowerTier = new THREE.Mesh(lowerGeo, brushedMetalMat);
    podiumGroup.add(lowerTier);

    // Lower Tier Glowing Cyan Ring
    const lowerRingGeo = new THREE.TorusGeometry(4.82, 0.045, 16, 120);
    const neonCyanMat = new THREE.MeshBasicMaterial({ color: 0x38bdf8 });
    const lowerRing = new THREE.Mesh(lowerRingGeo, neonCyanMat);
    lowerRing.rotation.x = Math.PI / 2;
    lowerRing.position.y = 0.22;
    podiumGroup.add(lowerRing);

    // Upper Tier Cylinder
    const upperGeo = new THREE.CylinderGeometry(3.3, 3.6, 0.4, 64);
    const upperMat = new THREE.MeshStandardMaterial({
      color: 0x2563eb,
      metalness: 0.7,
      roughness: 0.18,
    });
    const upperTier = new THREE.Mesh(upperGeo, upperMat);
    upperTier.position.y = 0.4;
    podiumGroup.add(upperTier);

    // Upper Tier Glowing White-Cyan Ring
    const upperRingGeo = new THREE.TorusGeometry(3.62, 0.04, 16, 120);
    const neonWhiteMat = new THREE.MeshBasicMaterial({ color: 0xe0f2fe });
    const upperRing = new THREE.Mesh(upperRingGeo, neonWhiteMat);
    upperRing.rotation.x = Math.PI / 2;
    upperRing.position.y = 0.6;
    podiumGroup.add(upperRing);

    // Top Mirror Disc
    const mirrorGeo = new THREE.CylinderGeometry(3.1, 3.1, 0.05, 64);
    const mirrorMat = new THREE.MeshStandardMaterial({
      color: 0x60a5fa,
      metalness: 0.95,
      roughness: 0.05,
    });
    const mirrorDisc = new THREE.Mesh(mirrorGeo, mirrorMat);
    mirrorDisc.position.y = 0.62;
    podiumGroup.add(mirrorDisc);

    // 4. Central Floating 3D Crystal Glass Cube
    const crystalGroup = new THREE.Group();
    crystalGroup.position.set(0, 0.4, 0);
    scene.add(crystalGroup);

    const cubeGeo = new THREE.BoxGeometry(2.1, 2.1, 2.1);
    const glassMat = new THREE.MeshPhysicalMaterial({
      color: 0xf0f9ff,
      transmission: 0.92,
      opacity: 0.95,
      transparent: true,
      roughness: 0.06,
      ior: 1.52,
      thickness: 1.8,
      specularIntensity: 1.0,
      specularColor: new THREE.Color(0xffffff),
    });
    const crystalCube = new THREE.Mesh(cubeGeo, glassMat);
    crystalGroup.add(crystalCube);

    // Interlocking 3D Hexagon Emblem inside the Crystal Cube
    const logoGroup = new THREE.Group();
    crystalGroup.add(logoGroup);

    const ringMat1 = new THREE.MeshStandardMaterial({
      color: 0x0284c7,
      metalness: 0.9,
      roughness: 0.15,
      emissive: 0x0369a1,
      emissiveIntensity: 0.4,
    });
    const ringMat2 = new THREE.MeshStandardMaterial({
      color: 0x38bdf8,
      metalness: 0.9,
      roughness: 0.15,
      emissive: 0x0284c7,
      emissiveIntensity: 0.4,
    });

    const torusGeo = new THREE.TorusGeometry(0.5, 0.12, 16, 48);
    const leftRing = new THREE.Mesh(torusGeo, ringMat1);
    leftRing.position.set(-0.32, 0.1, 0);
    leftRing.rotation.y = 0.3;
    logoGroup.add(leftRing);

    const rightRing = new THREE.Mesh(torusGeo, ringMat2);
    rightRing.position.set(0.32, -0.1, 0);
    rightRing.rotation.y = -0.3;
    logoGroup.add(rightRing);

    // 5. Orbital Elliptical Path & Orbiting Glass Icon Cubes
    const orbitCurve = new THREE.EllipseCurve(0, 0, 4.3, 1.9, 0, 2 * Math.PI, false, 0);
    const orbitPoints = orbitCurve.getPoints(120);
    const orbitGeo = new THREE.BufferGeometry().setFromPoints(
      orbitPoints.map((p) => new THREE.Vector3(p.x, 0, p.y))
    );
    const orbitLineMat = new THREE.LineBasicMaterial({
      color: 0x7dd3fc,
      transparent: true,
      opacity: 0.55,
    });
    const orbitLine = new THREE.Line(orbitGeo, orbitLineMat);
    orbitLine.rotation.x = THREE.MathUtils.degToRad(20);
    orbitLine.position.y = 0.4;
    scene.add(orbitLine);

    // Helper: Create Vector Icon Textures for Cubes
    const createIconTexture = (iconName: string): THREE.CanvasTexture => {
      const canvas = document.createElement('canvas');
      canvas.width = 256;
      canvas.height = 256;
      const ctx = canvas.getContext('2d')!;

      // Background gradient
      const grad = ctx.createLinearGradient(0, 0, 256, 256);
      grad.addColorStop(0, '#e0f2fe');
      grad.addColorStop(1, '#38bdf8');
      ctx.fillStyle = grad;
      ctx.fillRect(0, 0, 256, 256);

      // Icon drawing
      ctx.strokeStyle = '#0284c7';
      ctx.fillStyle = '#0284c7';
      ctx.lineWidth = 14;
      ctx.lineCap = 'round';
      ctx.lineJoin = 'round';

      const cx = 128;
      const cy = 128;

      if (iconName === 'chat') {
        ctx.beginPath();
        ctx.arc(cx, cy - 10, 60, 0, Math.PI * 2);
        ctx.fill();
        ctx.beginPath();
        ctx.moveTo(cx - 30, cy + 30);
        ctx.lineTo(cx - 50, cy + 70);
        ctx.lineTo(cx, cy + 45);
        ctx.fill();
      } else if (iconName === 'video') {
        ctx.fillRect(cx - 60, cy - 40, 75, 80);
        ctx.beginPath();
        ctx.moveTo(cx + 25, cy - 25);
        ctx.lineTo(cx + 65, cy - 45);
        ctx.lineTo(cx + 65, cy + 45);
        ctx.lineTo(cx + 25, cy + 25);
        ctx.closePath();
        ctx.fill();
      } else if (iconName === 'monitor') {
        ctx.strokeRect(cx - 60, cy - 50, 120, 80);
        ctx.fillRect(cx - 15, cy + 35, 30, 25);
        ctx.fillRect(cx - 40, cy + 60, 80, 12);
      } else if (iconName === 'headset') {
        ctx.beginPath();
        ctx.arc(cx, cy - 10, 50, Math.PI, 0, false);
        ctx.stroke();
        ctx.fillRect(cx - 60, cy - 20, 24, 45);
        ctx.fillRect(cx + 36, cy - 20, 24, 45);
      } else if (iconName === 'shield') {
        ctx.beginPath();
        ctx.moveTo(cx, cy - 60);
        ctx.lineTo(cx + 50, cy - 35);
        ctx.lineTo(cx + 40, cy + 30);
        ctx.lineTo(cx, cy + 65);
        ctx.lineTo(cx - 40, cy + 30);
        ctx.lineTo(cx - 50, cy - 35);
        ctx.closePath();
        ctx.fill();
      } else if (iconName === 'phone') {
        ctx.beginPath();
        ctx.arc(cx, cy, 45, 0, Math.PI * 2);
        ctx.stroke();
        ctx.fillRect(cx - 15, cy - 35, 30, 70);
      } else {
        // team
        ctx.beginPath();
        ctx.arc(cx, cy - 25, 25, 0, Math.PI * 2);
        ctx.fill();
        ctx.beginPath();
        ctx.arc(cx, cy + 50, 45, Math.PI, 0, false);
        ctx.fill();
      }

      return new THREE.CanvasTexture(canvas);
    };

    const iconNames = ['chat', 'video', 'monitor', 'headset', 'shield', 'team', 'phone'];
    const orbitCubes: THREE.Mesh[] = [];

    const orbitBoxGeo = new THREE.BoxGeometry(0.68, 0.68, 0.68);

    iconNames.forEach((name) => {
      const tex = createIconTexture(name);
      const cubeMat = new THREE.MeshStandardMaterial({
        map: tex,
        color: 0xffffff,
        metalness: 0.3,
        roughness: 0.15,
      });
      const cube = new THREE.Mesh(orbitBoxGeo, cubeMat);
      orbitCubes.push(cube);
      scene.add(cube);
    });

    // 6. Holographic Enterprise Figure Silhouettes (Leader & Specialist)
    const createHologramFigure = (isLeft: boolean): THREE.Group => {
      const figure = new THREE.Group();
      figure.position.set(isLeft ? -2.2 : 2.2, -1.1, isLeft ? 1.4 : 1.2);

      const holoMat = new THREE.MeshBasicMaterial({
        color: isLeft ? 0x38bdf8 : 0x818cf8,
        transparent: true,
        opacity: 0.8,
      });

      // Head
      const head = new THREE.Mesh(new THREE.SphereGeometry(0.24, 24, 24), holoMat);
      head.position.y = 1.6;
      figure.add(head);

      // Torso (Suit / Corporate Blazer)
      const torso = new THREE.Mesh(new THREE.CylinderGeometry(0.24, 0.38, 0.9, 16), holoMat);
      torso.position.y = 0.95;
      figure.add(torso);

      // Lower Body / Legs
      const legs = new THREE.Mesh(new THREE.CylinderGeometry(0.35, 0.28, 0.9, 16), holoMat);
      legs.position.y = 0.25;
      figure.add(legs);

      // Vertical Scan Ring
      const scanRing = new THREE.Mesh(
        new THREE.TorusGeometry(0.5, 0.02, 16, 48),
        new THREE.MeshBasicMaterial({ color: 0xffffff, transparent: true, opacity: 0.9 })
      );
      scanRing.rotation.x = Math.PI / 2;
      scanRing.position.y = 1.0;
      figure.add(scanRing);

      return figure;
    };

    const leaderFigure = createHologramFigure(true);
    const specialistFigure = createHologramFigure(false);
    scene.add(leaderFigure);
    scene.add(specialistFigure);

    // 7. Mouse Interaction & Responsive Parallax
    let targetMouseX = 0;
    let targetMouseY = 0;
    let currentMouseX = 0;
    let currentMouseY = 0;

    const onPointerMove = (e: MouseEvent) => {
      const rect = container.getBoundingClientRect();
      targetMouseX = ((e.clientX - rect.left) / rect.width - 0.5) * 2;
      targetMouseY = ((e.clientY - rect.top) / rect.height - 0.5) * 2;
    };

    window.addEventListener('mousemove', onPointerMove);

    // Click handler for 3D elements
    const onClick = (e: MouseEvent) => {
      const rect = container.getBoundingClientRect();
      const x = ((e.clientX - rect.left) / rect.width) * 2 - 1;
      const y = -((e.clientY - rect.top) / rect.height) * 2 + 1;

      const raycaster = new THREE.Raycaster();
      raycaster.setFromCamera(new THREE.Vector2(x, y), camera);
      const intersects = raycaster.intersectObjects([crystalCube, ...orbitCubes], true);
      if (intersects.length > 0) {
        onSelectAction();
      }
    };

    renderer.domElement.addEventListener('click', onClick);

    // 8. Animation Render Loop
    let animationFrameId: number;
    let clock = new THREE.Clock();

    const animate = () => {
      animationFrameId = requestAnimationFrame(animate);
      const elapsedTime = clock.getElapsedTime();

      // Smooth camera dampening (Parallax)
      currentMouseX += (targetMouseX - currentMouseX) * 0.04;
      currentMouseY += (targetMouseY - currentMouseY) * 0.04;

      camera.position.x = currentMouseX * 1.6;
      camera.position.y = 2.2 + -currentMouseY * 1.1;
      camera.lookAt(0, 0.2, 0);

      // Central crystal floating & breathing rotation
      crystalGroup.position.y = 0.4 + Math.sin(elapsedTime * 1.6) * 0.15;
      crystalCube.rotation.x = Math.sin(elapsedTime * 0.8) * 0.08;
      crystalCube.rotation.y = elapsedTime * 0.35;
      logoGroup.rotation.y = -elapsedTime * 0.5;

      // Orbiting glass cubes movement
      const orbitTilt = THREE.MathUtils.degToRad(20);
      orbitCubes.forEach((cube, i) => {
        const angle = elapsedTime * 0.4 + i * ((2 * Math.PI) / 7);
        const radiusA = 4.3;
        const radiusB = 1.9;

        const rawX = Math.cos(angle) * radiusA;
        const rawZ = Math.sin(angle) * radiusB;

        // Apply orbit tilt (rotate along X axis)
        const posX = rawX;
        const posY = 0.4 + -rawZ * Math.sin(orbitTilt) + Math.sin(elapsedTime * 3 + i) * 0.06;
        const posZ = rawZ * Math.cos(orbitTilt);

        cube.position.set(posX, posY, posZ);
        cube.rotation.x = elapsedTime * 0.7 + i;
        cube.rotation.y = elapsedTime * 0.9 + i;
      });

      // Hologram pulse
      const pulse = 0.75 + Math.sin(elapsedTime * 3) * 0.15;
      (leaderFigure.children[0] as THREE.Mesh).scale.setScalar(pulse);
      (specialistFigure.children[0] as THREE.Mesh).scale.setScalar(pulse);

      renderer.render(scene, camera);
    };

    animate();

    // 9. Resize Handling
    const handleResize = () => {
      if (!container) return;
      camera.aspect = container.clientWidth / container.clientHeight;
      camera.updateProjectionMatrix();
      renderer.setSize(container.clientWidth, container.clientHeight);
    };

    window.addEventListener('resize', handleResize);

    return () => {
      window.removeEventListener('mousemove', onPointerMove);
      window.removeEventListener('resize', handleResize);
      renderer.domElement.removeEventListener('click', onClick);
      cancelAnimationFrame(animationFrameId);
      renderer.dispose();
      if (renderer.domElement.parentNode) {
        renderer.domElement.parentNode.removeChild(renderer.domElement);
      }
    };
  }, [onSelectAction]);

  return (
    <div
      ref={mountRef}
      className="relative w-full h-[520px] lg:h-[580px] cursor-pointer select-none"
      title="Khối pha lê tương tác 3D - Bấm để Đăng Nhập"
    />
  );
};
