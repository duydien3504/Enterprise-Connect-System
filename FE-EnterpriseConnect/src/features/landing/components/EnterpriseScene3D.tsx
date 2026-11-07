import React, { useEffect, useRef } from 'react';
import * as THREE from 'three';
import { RoundedBoxGeometry } from 'three/examples/jsm/geometries/RoundedBoxGeometry.js';

export const EnterpriseScene3D: React.FC = () => {
  const mountRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const container = mountRef.current;
    if (!container) return;

    // 1. Scene, Camera, Renderer Setup
    const scene = new THREE.Scene();

    const camera = new THREE.PerspectiveCamera(
      40,
      container.clientWidth / container.clientHeight,
      0.1,
      1000
    );
    camera.position.set(0, 2.0, 10.5);

    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    renderer.setSize(container.clientWidth, container.clientHeight);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.3;
    container.appendChild(renderer.domElement);

    // 2. Realistic Studio & Atmospheric Lighting
    const ambientLight = new THREE.AmbientLight(0xdbeafe, 1.8);
    scene.add(ambientLight);

    const mainSun = new THREE.DirectionalLight(0xffffff, 3.5);
    mainSun.position.set(10, 16, 12);
    scene.add(mainSun);

    const rimLight = new THREE.DirectionalLight(0x38bdf8, 2.2);
    rimLight.position.set(-12, 6, -8);
    scene.add(rimLight);

    const bottomGlow = new THREE.PointLight(0x0284c7, 3.5, 14);
    bottomGlow.position.set(0, -1.2, 0);
    scene.add(bottomGlow);

    const centerSparkle = new THREE.PointLight(0x38bdf8, 2.8, 10);
    centerSparkle.position.set(0, 0.6, 1.5);
    scene.add(centerSparkle);

    // 3. Multi-tier Circular Terrace Podium
    const podiumGroup = new THREE.Group();
    podiumGroup.position.set(0, -2.3, 0);
    scene.add(podiumGroup);

    // Tier 1: Lower Brushed Metallic Base
    const lowerGeo = new THREE.CylinderGeometry(4.3, 4.7, 0.45, 64);
    const metalMat = new THREE.MeshStandardMaterial({
      color: 0x172554,
      metalness: 0.88,
      roughness: 0.22,
    });
    const lowerTier = new THREE.Mesh(lowerGeo, metalMat);
    podiumGroup.add(lowerTier);

    // Lower Tier Neon Cyan Ring
    const lowerRingGeo = new THREE.TorusGeometry(4.72, 0.04, 16, 120);
    const neonCyanMat = new THREE.MeshBasicMaterial({ color: 0x38bdf8 });
    const lowerRing = new THREE.Mesh(lowerRingGeo, neonCyanMat);
    lowerRing.rotation.x = Math.PI / 2;
    lowerRing.position.y = 0.23;
    podiumGroup.add(lowerRing);

    // Tier 2: Mid Beveled Platform
    const midGeo = new THREE.CylinderGeometry(3.3, 3.6, 0.38, 64);
    const midMat = new THREE.MeshStandardMaterial({
      color: 0x1d4ed8,
      metalness: 0.75,
      roughness: 0.16,
    });
    const midTier = new THREE.Mesh(midGeo, midMat);
    midTier.position.y = 0.4;
    podiumGroup.add(midTier);

    // Upper Tier Pure Glow White-Cyan Ring
    const upperRingGeo = new THREE.TorusGeometry(3.62, 0.038, 16, 120);
    const neonWhiteMat = new THREE.MeshBasicMaterial({ color: 0xf0f9ff });
    const upperRing = new THREE.Mesh(upperRingGeo, neonWhiteMat);
    upperRing.rotation.x = Math.PI / 2;
    upperRing.position.y = 0.59;
    podiumGroup.add(upperRing);

    // Tier 3: Top Mirror Disc
    const mirrorGeo = new THREE.CylinderGeometry(3.1, 3.1, 0.04, 64);
    const mirrorMat = new THREE.MeshStandardMaterial({
      color: 0x3b82f6,
      metalness: 0.95,
      roughness: 0.04,
    });
    const mirrorDisc = new THREE.Mesh(mirrorGeo, mirrorMat);
    mirrorDisc.position.y = 0.61;
    podiumGroup.add(mirrorDisc);

    // 4. Central Large 3D Crystal Cube (Physical Rounded Glass)
    const crystalGroup = new THREE.Group();
    crystalGroup.position.set(0, 0.5, 0);
    scene.add(crystalGroup);

    const crystalGeo = new RoundedBoxGeometry(2.1, 2.1, 2.1, 4, 0.35);
    const crystalMat = new THREE.MeshPhysicalMaterial({
      color: 0xf8fafc,
      transmission: 0.93,
      opacity: 0.96,
      transparent: true,
      roughness: 0.05,
      ior: 1.54,
      thickness: 1.9,
      specularIntensity: 1.0,
      specularColor: new THREE.Color(0xffffff),
      clearcoat: 1.0,
      clearcoatRoughness: 0.04,
    });
    const centralCube = new THREE.Mesh(crystalGeo, crystalMat);
    crystalGroup.add(centralCube);

    // 3D Interlocking Hexagon Brand Logo inside Central Crystal
    const logoGroup = new THREE.Group();
    crystalGroup.add(logoGroup);

    const brandMat1 = new THREE.MeshStandardMaterial({
      color: 0x0284c7,
      metalness: 0.92,
      roughness: 0.12,
      emissive: 0x0284c7,
      emissiveIntensity: 0.6,
    });
    const brandMat2 = new THREE.MeshStandardMaterial({
      color: 0x38bdf8,
      metalness: 0.92,
      roughness: 0.12,
      emissive: 0x38bdf8,
      emissiveIntensity: 0.6,
    });

    const linkGeo = new THREE.TorusGeometry(0.52, 0.12, 16, 48);
    const linkLeft = new THREE.Mesh(linkGeo, brandMat1);
    linkLeft.position.set(-0.35, 0.12, 0);
    linkLeft.rotation.y = 0.35;
    logoGroup.add(linkLeft);

    const linkRight = new THREE.Mesh(linkGeo, brandMat2);
    linkRight.position.set(0.35, -0.12, 0);
    linkRight.rotation.y = -0.35;
    logoGroup.add(linkRight);

    // 5. 3D Elliptical Orbiting Ring & 7 Crystal Glass Icon Cubes
    const orbitCurve = new THREE.EllipseCurve(0, 0, 4.4, 1.95, 0, 2 * Math.PI, false, 0);
    const orbitPoints = orbitCurve.getPoints(120);
    const orbitGeo = new THREE.BufferGeometry().setFromPoints(
      orbitPoints.map((p) => new THREE.Vector3(p.x, 0, p.y))
    );
    const orbitLineMat = new THREE.LineBasicMaterial({
      color: 0x7dd3fc,
      transparent: true,
      opacity: 0.65,
    });
    const orbitLine = new THREE.Line(orbitGeo, orbitLineMat);
    orbitLine.rotation.x = THREE.MathUtils.degToRad(22);
    orbitLine.position.y = 0.5;
    scene.add(orbitLine);

    // High-Resolution Crisp 3D Icon Textures (Vector Quality with Chamfer & Gloss)
    const createCrispIconTexture = (type: string): THREE.CanvasTexture => {
      const cv = document.createElement('canvas');
      cv.width = 512;
      cv.height = 512;
      const ctx = cv.getContext('2d')!;

      // Rounded gradient backdrop
      const bgGrad = ctx.createLinearGradient(0, 0, 512, 512);
      bgGrad.addColorStop(0, 'rgba(240, 249, 255, 0.95)');
      bgGrad.addColorStop(0.5, 'rgba(186, 230, 253, 0.92)');
      bgGrad.addColorStop(1, 'rgba(56, 189, 248, 0.95)');
      ctx.fillStyle = bgGrad;

      // Rounded rect
      const r = 80;
      ctx.beginPath();
      ctx.roundRect(24, 24, 464, 464, r);
      ctx.fill();

      // Border highlight
      ctx.strokeStyle = '#ffffff';
      ctx.lineWidth = 14;
      ctx.stroke();

      // Inner shadow border
      ctx.strokeStyle = 'rgba(2, 132, 199, 0.35)';
      ctx.lineWidth = 6;
      ctx.stroke();

      // Draw vector enterprise glyphs
      ctx.fillStyle = '#0284c7';
      ctx.strokeStyle = '#0284c7';
      ctx.lineWidth = 26;
      ctx.lineCap = 'round';
      ctx.lineJoin = 'round';

      const cx = 256;
      const cy = 256;

      if (type === 'chat') {
        // Message bubble with 3 dots
        ctx.beginPath();
        ctx.roundRect(116, 136, 280, 190, 50);
        ctx.fill();
        ctx.beginPath();
        ctx.moveTo(170, 320);
        ctx.lineTo(130, 380);
        ctx.lineTo(230, 320);
        ctx.fill();
        // 3 white dots
        ctx.fillStyle = '#ffffff';
        for (let i = 0; i < 3; i++) {
          ctx.beginPath();
          ctx.arc(196 + i * 60, 230, 16, 0, Math.PI * 2);
          ctx.fill();
        }
      } else if (type === 'video') {
        // Video camera with lens
        ctx.beginPath();
        ctx.roundRect(100, 156, 210, 160, 36);
        ctx.fill();
        ctx.beginPath();
        ctx.moveTo(330, 200);
        ctx.lineTo(410, 150);
        ctx.lineTo(410, 320);
        ctx.lineTo(330, 270);
        ctx.closePath();
        ctx.fill();
      } else if (type === 'monitor') {
        // Desktop Screen
        ctx.beginPath();
        ctx.roundRect(110, 130, 292, 180, 28);
        ctx.stroke();
        ctx.fillRect(236, 320, 40, 60);
        ctx.beginPath();
        ctx.roundRect(180, 375, 152, 20, 10);
        ctx.fill();
      } else if (type === 'headset') {
        // Headset / Customer Support
        ctx.beginPath();
        ctx.arc(cx, cy - 10, 110, Math.PI, 0, false);
        ctx.stroke();
        ctx.fillRect(116, cy - 30, 45, 90);
        ctx.fillRect(351, cy - 30, 45, 90);
        ctx.beginPath();
        ctx.moveTo(370, cy + 50);
        ctx.lineTo(370, cy + 90);
        ctx.lineTo(290, cy + 90);
        ctx.stroke();
        ctx.beginPath();
        ctx.arc(275, cy + 90, 16, 0, Math.PI * 2);
        ctx.fill();
      } else if (type === 'shield') {
        // Security Shield with checkmark
        ctx.beginPath();
        ctx.moveTo(cx, 100);
        ctx.lineTo(370, 140);
        ctx.quadraticCurveTo(375, 300, cx, 400);
        ctx.quadraticCurveTo(137, 300, 142, 140);
        ctx.closePath();
        ctx.fill();
        // White checkmark
        ctx.strokeStyle = '#ffffff';
        ctx.lineWidth = 26;
        ctx.beginPath();
        ctx.moveTo(195, 245);
        ctx.lineTo(235, 290);
        ctx.lineTo(320, 195);
        ctx.stroke();
      } else if (type === 'team') {
        // 3 User Figures / Team
        ctx.beginPath();
        ctx.arc(cx, 160, 45, 0, Math.PI * 2);
        ctx.fill();
        ctx.beginPath();
        ctx.arc(cx, 320, 85, Math.PI, 0, false);
        ctx.fill();
        // Left mini figure
        ctx.beginPath();
        ctx.arc(cx - 105, 190, 32, 0, Math.PI * 2);
        ctx.fill();
        // Right mini figure
        ctx.beginPath();
        ctx.arc(cx + 105, 190, 32, 0, Math.PI * 2);
        ctx.fill();
      } else {
        // Phone Handset
        ctx.beginPath();
        ctx.arc(cx, cy, 105, 0, Math.PI * 2);
        ctx.stroke();
        ctx.beginPath();
        ctx.roundRect(cx - 30, cy - 70, 60, 140, 24);
        ctx.fill();
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(cx - 16, cy - 40, 32, 50);
      }

      const tex = new THREE.CanvasTexture(cv);
      tex.colorSpace = THREE.SRGBColorSpace;
      return tex;
    };

    const iconTypes = ['chat', 'video', 'monitor', 'headset', 'shield', 'team', 'phone'];
    const orbitCubeMeshes: THREE.Group[] = [];
    const cubeGeo = new RoundedBoxGeometry(0.85, 0.85, 0.85, 4, 0.16);

    iconTypes.forEach((type) => {
      const cubeGroup = new THREE.Group();

      // Outer Real Glass Shell
      const glassShellMat = new THREE.MeshPhysicalMaterial({
        color: 0xf0f9ff,
        transmission: 0.92,
        opacity: 0.95,
        transparent: true,
        roughness: 0.05,
        ior: 1.52,
        thickness: 1.1,
        specularIntensity: 1.0,
        specularColor: new THREE.Color(0xffffff),
        clearcoat: 1.0,
      });
      const glassShell = new THREE.Mesh(cubeGeo, glassShellMat);
      cubeGroup.add(glassShell);

      // Inner Glowing Core with Vector Icon Texture
      const iconTex = createCrispIconTexture(type);
      const coreGeo = new THREE.PlaneGeometry(0.72, 0.72);
      const coreMat = new THREE.MeshBasicMaterial({
        map: iconTex,
        transparent: true,
        opacity: 0.98,
        side: THREE.DoubleSide,
      });
      const iconPlaneFront = new THREE.Mesh(coreGeo, coreMat);
      iconPlaneFront.position.z = 0.01;
      cubeGroup.add(iconPlaneFront);

      const iconPlaneBack = new THREE.Mesh(coreGeo, coreMat);
      iconPlaneBack.position.z = -0.01;
      iconPlaneBack.rotation.y = Math.PI;
      cubeGroup.add(iconPlaneBack);

      orbitCubeMeshes.push(cubeGroup);
      scene.add(cubeGroup);
    });

    // 6. 100% Real Human Corporate Executive Leader Standing on Terrace
    const humanGroup = new THREE.Group();
    humanGroup.position.set(-2.5, -0.7, 1.6);
    scene.add(humanGroup);

    // Realistic Floor Contact Shadow
    const shadowCanvas = document.createElement('canvas');
    shadowCanvas.width = 128;
    shadowCanvas.height = 128;
    const sCtx = shadowCanvas.getContext('2d')!;
    const sGrad = sCtx.createRadialGradient(64, 64, 10, 64, 64, 60);
    sGrad.addColorStop(0, 'rgba(15, 23, 42, 0.85)');
    sGrad.addColorStop(0.6, 'rgba(15, 23, 42, 0.4)');
    sGrad.addColorStop(1, 'rgba(15, 23, 42, 0)');
    sCtx.fillStyle = sGrad;
    sCtx.fillRect(0, 0, 128, 128);

    const shadowTex = new THREE.CanvasTexture(shadowCanvas);
    const shadowPlane = new THREE.Mesh(
      new THREE.PlaneGeometry(1.9, 0.9),
      new THREE.MeshBasicMaterial({
        map: shadowTex,
        transparent: true,
        opacity: 0.8,
        depthWrite: false,
      })
    );
    shadowPlane.rotation.x = -Math.PI / 2;
    shadowPlane.position.y = -0.92;
    humanGroup.add(shadowPlane);

    // 100% Real Corporate Executive Leader Portrait Mesh
    const textureLoader = new THREE.TextureLoader();
    textureLoader.load(
      '/executive-3d.jpg',
      (texture) => {
        texture.colorSpace = THREE.SRGBColorSpace;
        // Glassmorphic Stand / Holographic Frame with Rounded Bevels
        const frameWidth = 1.65;
        const frameHeight = 2.45;

        // Executive Plane
        const humanMat = new THREE.MeshBasicMaterial({
          map: texture,
          transparent: true,
          opacity: 0.98,
        });
        const humanMesh = new THREE.Mesh(
          new THREE.PlaneGeometry(frameWidth, frameHeight),
          humanMat
        );
        humanMesh.position.y = 0.35;
        humanGroup.add(humanMesh);

        // Surrounding Glass Border Frame
        const frameEdges = new THREE.LineSegments(
          new THREE.EdgesGeometry(new THREE.PlaneGeometry(frameWidth + 0.08, frameHeight + 0.08)),
          new THREE.LineBasicMaterial({ color: 0x7dd3fc, transparent: true, opacity: 0.85 })
        );
        frameEdges.position.y = 0.35;
        humanGroup.add(frameEdges);
      },
      undefined,
      () => {
        // Fallback gracefully if image is loading
      }
    );

    // 7. Subtle Ambient Floating Cyber Motes
    const particleCount = 70;
    const particlePositions = new Float32Array(particleCount * 3);
    for (let i = 0; i < particleCount; i++) {
      particlePositions[i * 3] = (Math.random() - 0.5) * 8;
      particlePositions[i * 3 + 1] = Math.random() * 5 - 2;
      particlePositions[i * 3 + 2] = (Math.random() - 0.5) * 6;
    }
    const particleGeo = new THREE.BufferGeometry();
    particleGeo.setAttribute('position', new THREE.BufferAttribute(particlePositions, 3));
    const particleMat = new THREE.PointsMaterial({
      color: 0x38bdf8,
      size: 0.06,
      transparent: true,
      opacity: 0.7,
      blending: THREE.AdditiveBlending,
    });
    const particles = new THREE.Points(particleGeo, particleMat);
    scene.add(particles);

    // 8. Smooth Mouse Parallax Tracking
    let mouseX = 0;
    let mouseY = 0;
    let targetX = 0;
    let targetY = 0;

    const handleMouseMove = (e: MouseEvent) => {
      const rect = container.getBoundingClientRect();
      const x = ((e.clientX - rect.left) / rect.width - 0.5) * 2;
      const y = ((e.clientY - rect.top) / rect.height - 0.5) * 2;
      targetX = x * 1.4;
      targetY = -y * 1.0;
    };

    window.addEventListener('mousemove', handleMouseMove);

    // 9. Animation Loop
    let animId: number;
    let clock = new THREE.Clock();

    const animate = () => {
      animId = requestAnimationFrame(animate);
      const elapsedTime = clock.getElapsedTime();

      // Smooth mouse lerp
      mouseX += (targetX - mouseX) * 0.05;
      mouseY += (targetY - mouseY) * 0.05;

      camera.position.x = mouseX * 1.2;
      camera.position.y = 2.0 + mouseY * 0.8;
      camera.lookAt(0, 0.3, 0);

      // Central crystal rotation & breathing float
      centralCube.rotation.y = elapsedTime * 0.35;
      centralCube.rotation.x = Math.sin(elapsedTime * 0.4) * 0.08;
      logoGroup.rotation.y = -elapsedTime * 0.45;
      crystalGroup.position.y = 0.5 + Math.sin(elapsedTime * 1.5) * 0.08;

      // Orbiting cubes movement
      const orbitSpeed = 0.42;
      const total = iconTypes.length;

      orbitCubeMeshes.forEach((mesh, index) => {
        const theta = (index / total) * Math.PI * 2 + elapsedTime * orbitSpeed;
        const ox = Math.cos(theta) * 4.4;
        const oz = Math.sin(theta) * 1.95;

        // Apply 22 deg inclination
        const inc = THREE.MathUtils.degToRad(22);
        const rx = ox;
        const ry = 0.5 - oz * Math.sin(inc) + Math.sin(elapsedTime * 2 + index) * 0.08;
        const rz = oz * Math.cos(inc);

        mesh.position.set(rx, ry, rz);
        mesh.rotation.y = theta + Math.PI / 2;
        mesh.rotation.x = Math.sin(elapsedTime * 1.8 + index) * 0.15;
      });

      // Human subtle idle breathing
      humanGroup.position.y = -0.7 + Math.sin(elapsedTime * 1.2) * 0.03;

      // Particle rising drift
      const positions = particleGeo.attributes.position.array as Float32Array;
      for (let i = 0; i < particleCount; i++) {
        positions[i * 3 + 1] += 0.008;
        if (positions[i * 3 + 1] > 3.5) {
          positions[i * 3 + 1] = -2.2;
        }
      }
      particleGeo.attributes.position.needsUpdate = true;

      renderer.render(scene, camera);
    };

    animate();

    // Resize Handler
    const handleResize = () => {
      if (!container) return;
      camera.aspect = container.clientWidth / container.clientHeight;
      camera.updateProjectionMatrix();
      renderer.setSize(container.clientWidth, container.clientHeight);
    };

    window.addEventListener('resize', handleResize);

    return () => {
      window.removeEventListener('mousemove', handleMouseMove);
      window.removeEventListener('resize', handleResize);
      cancelAnimationFrame(animId);
      if (renderer.domElement.parentElement) {
        renderer.domElement.parentElement.removeChild(renderer.domElement);
      }
      renderer.dispose();
    };
  }, []);

  return <div ref={mountRef} className="w-full h-full pointer-events-none select-none" />;
};
