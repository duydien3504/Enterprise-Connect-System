import React, { useEffect, useRef } from 'react';
import * as THREE from 'three';

export const EnterpriseScene3D: React.FC = () => {
  const mountRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const container = mountRef.current;
    if (!container) return;

    // 1. Scene, Camera, Renderer Setup (Full-Page 3D)
    const scene = new THREE.Scene();

    // Solid Corporate Blue Background (TUYỆT ĐỐI KHÔNG DÙNG GRADIENT)
    const solidBgColor = 0x071e3d;
    scene.background = new THREE.Color(solidBgColor);
    scene.fog = new THREE.FogExp2(solidBgColor, 0.035);

    const camera = new THREE.PerspectiveCamera(
      50,
      window.innerWidth / window.innerHeight,
      0.1,
      1000
    );
    camera.position.set(0, 0, 22);

    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false });
    renderer.setSize(window.innerWidth, window.innerHeight);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.2;
    container.appendChild(renderer.domElement);

    // 2. Solid Blue Lighting (No Gradients)
    const ambientLight = new THREE.AmbientLight(0x1e3a8a, 2.2);
    scene.add(ambientLight);

    const coreLight = new THREE.PointLight(0x0065ff, 4.0, 45);
    coreLight.position.set(0, 0, 5);
    scene.add(coreLight);

    const rimLight = new THREE.DirectionalLight(0x38bdf8, 2.5);
    rimLight.position.set(15, 20, 15);
    scene.add(rimLight);

    // 3. 3D Enterprise Node Mesh (Mạng Lưới Liên Kết Doanh Nghiệp)
    const nodeCount = 85;
    const nodePositions: THREE.Vector3[] = [];
    const nodeVelocities: THREE.Vector3[] = [];

    // Distribute nodes in a spacious 3D sphere volume
    for (let i = 0; i < nodeCount; i++) {
      const radius = 9 + Math.random() * 8;
      const theta = Math.random() * Math.PI * 2;
      const phi = Math.acos(2 * Math.random() - 1);

      const x = radius * Math.sin(phi) * Math.cos(theta);
      const y = radius * Math.sin(phi) * Math.sin(theta) * 0.75;
      const z = radius * Math.cos(phi) * 0.85;

      nodePositions.push(new THREE.Vector3(x, y, z));
      nodeVelocities.push(
        new THREE.Vector3(
          (Math.random() - 0.5) * 0.012,
          (Math.random() - 0.5) * 0.012,
          (Math.random() - 0.5) * 0.012
        )
      );
    }

    // Nodes Instanced Geometry for high performance (60 FPS)
    const nodeGeo = new THREE.SphereGeometry(0.18, 16, 16);
    const nodeMat = new THREE.MeshStandardMaterial({
      color: 0x38bdf8,
      emissive: 0x0052cc,
      emissiveIntensity: 0.8,
      metalness: 0.8,
      roughness: 0.2,
    });
    const nodeMesh = new THREE.InstancedMesh(nodeGeo, nodeMat, nodeCount);
    scene.add(nodeMesh);

    // 6 Major Enterprise Core Hubs (Trụ sở chính & Trung tâm dữ liệu kết nối)
    const hubMeshes: THREE.Mesh[] = [];
    const hubGeo = new THREE.IcosahedronGeometry(0.65, 1);
    const hubMat = new THREE.MeshStandardMaterial({
      color: 0xffffff,
      emissive: 0x0052cc,
      emissiveIntensity: 0.9,
      wireframe: true,
    });

    const hubPositions = [
      new THREE.Vector3(-6, 3, -2),
      new THREE.Vector3(6, 3, -1),
      new THREE.Vector3(-7, -3.5, 1),
      new THREE.Vector3(7, -3.5, 2),
      new THREE.Vector3(0, 5.5, -4),
      new THREE.Vector3(0, -5.5, -3),
    ];

    hubPositions.forEach((pos) => {
      const hub = new THREE.Mesh(hubGeo, hubMat);
      hub.position.copy(pos);
      hubMeshes.push(hub);
      scene.add(hub);

      // Core Inner Glowing Sphere
      const innerCore = new THREE.Mesh(
        new THREE.SphereGeometry(0.35, 16, 16),
        new THREE.MeshBasicMaterial({ color: 0x0065ff })
      );
      hub.add(innerCore);
    });

    // Dynamic Connecting Network Lines
    const maxConnections = 350;
    const linePositions = new Float32Array(maxConnections * 2 * 3);
    const lineColors = new Float32Array(maxConnections * 2 * 3);

    const lineGeo = new THREE.BufferGeometry();
    lineGeo.setAttribute('position', new THREE.BufferAttribute(linePositions, 3));
    lineGeo.setAttribute('color', new THREE.BufferAttribute(lineColors, 3));

    const lineMat = new THREE.LineBasicMaterial({
      vertexColors: true,
      transparent: true,
      opacity: 0.75,
      blending: THREE.AdditiveBlending,
    });
    const networkLines = new THREE.LineSegments(lineGeo, lineMat);
    scene.add(networkLines);

    // 4. Central Enterprise Nexus Rings (Vòng Kết Nối Trung Tâm)
    const centralGroup = new THREE.Group();
    scene.add(centralGroup);

    const ringMat = new THREE.LineBasicMaterial({
      color: 0x1d4ed8,
      transparent: true,
      opacity: 0.6,
    });

    for (let r = 0; r < 3; r++) {
      const radius = 4.2 + r * 1.8;
      const pts: THREE.Vector3[] = [];
      const segments = 90;
      for (let s = 0; s <= segments; s++) {
        const a = (s / segments) * Math.PI * 2;
        pts.push(new THREE.Vector3(Math.cos(a) * radius, Math.sin(a) * radius, 0));
      }
      const rGeo = new THREE.BufferGeometry().setFromPoints(pts);
      const ring = new THREE.Line(rGeo, ringMat);
      ring.rotation.x = Math.PI * (0.25 + r * 0.15);
      ring.rotation.y = Math.PI * (0.15 * r);
      centralGroup.add(ring);
    }

    // 5. Data Packet Pulses Moving Through Enterprise Lines
    const packetCount = 20;
    const packetGeo = new THREE.BufferGeometry();
    const packetPosArray = new Float32Array(packetCount * 3);
    packetGeo.setAttribute('position', new THREE.BufferAttribute(packetPosArray, 3));
    const packetMat = new THREE.PointsMaterial({
      color: 0xffffff,
      size: 0.28,
      transparent: true,
      opacity: 0.95,
      blending: THREE.AdditiveBlending,
    });
    const packets = new THREE.Points(packetGeo, packetMat);
    scene.add(packets);

    const packetRoutes: { from: number; to: number; progress: number; speed: number }[] = [];
    for (let p = 0; p < packetCount; p++) {
      packetRoutes.push({
        from: Math.floor(Math.random() * nodeCount),
        to: Math.floor(Math.random() * nodeCount),
        progress: Math.random(),
        speed: 0.005 + Math.random() * 0.01,
      });
    }

    // 6. Interactive Mouse Tracking
    let targetMouseX = 0;
    let targetMouseY = 0;
    let currentMouseX = 0;
    let currentMouseY = 0;

    const handleMouseMove = (e: MouseEvent) => {
      targetMouseX = (e.clientX / window.innerWidth - 0.5) * 2;
      targetMouseY = (e.clientY / window.innerHeight - 0.5) * 2;
    };

    window.addEventListener('mousemove', handleMouseMove);

    // 7. Animation Loop (Smooth 60 FPS)
    let animId: number;
    const dummy = new THREE.Object3D();
    const clock = new THREE.Clock();

    const animate = () => {
      animId = requestAnimationFrame(animate);
      const elapsedTime = clock.getElapsedTime();

      // Smooth mouse lerp
      currentMouseX += (targetMouseX - currentMouseX) * 0.04;
      currentMouseY += (targetMouseY - currentMouseY) * 0.04;

      camera.position.x = currentMouseX * 3.5;
      camera.position.y = -currentMouseY * 2.8;
      camera.lookAt(0, 0, 0);

      // Rotate central nexus
      centralGroup.rotation.z = elapsedTime * 0.08;
      centralGroup.rotation.y = elapsedTime * 0.06;

      // Rotate major hubs
      hubMeshes.forEach((hub, idx) => {
        hub.rotation.x = elapsedTime * (0.3 + idx * 0.1);
        hub.rotation.y = elapsedTime * (0.4 + idx * 0.08);
      });

      // Update node positions with gentle floating kinetic movement
      for (let i = 0; i < nodeCount; i++) {
        const pos = nodePositions[i];
        const vel = nodeVelocities[i];

        pos.add(vel);

        // Keep within bounds
        const dist = pos.length();
        if (dist > 16 || dist < 6) {
          vel.reflect(pos.clone().normalize());
        }

        dummy.position.copy(pos);
        dummy.updateMatrix();
        nodeMesh.setMatrixAt(i, dummy.matrix);
      }
      nodeMesh.instanceMatrix.needsUpdate = true;

      // Update connecting lines dynamically
      let lineIndex = 0;
      const connectionDist = 4.8;
      const posArray = lineGeo.attributes.position.array as Float32Array;
      const colArray = lineGeo.attributes.color.array as Float32Array;

      // Solid color stops (Pure Blue and Cyan, NO gradients)
      const c1 = new THREE.Color(0x0052cc);
      const c2 = new THREE.Color(0x38bdf8);

      for (let i = 0; i < nodeCount && lineIndex < maxConnections; i++) {
        for (let j = i + 1; j < nodeCount && lineIndex < maxConnections; j++) {
          const d = nodePositions[i].distanceTo(nodePositions[j]);
          if (d < connectionDist) {
            const idx = lineIndex * 6;

            posArray[idx] = nodePositions[i].x;
            posArray[idx + 1] = nodePositions[i].y;
            posArray[idx + 2] = nodePositions[i].z;

            posArray[idx + 3] = nodePositions[j].x;
            posArray[idx + 4] = nodePositions[j].y;
            posArray[idx + 5] = nodePositions[j].z;

            colArray[idx] = c1.r;
            colArray[idx + 1] = c1.g;
            colArray[idx + 2] = c1.b;

            colArray[idx + 3] = c2.r;
            colArray[idx + 4] = c2.g;
            colArray[idx + 5] = c2.b;

            lineIndex++;
          }
        }
      }

      lineGeo.setDrawRange(0, lineIndex * 2);
      lineGeo.attributes.position.needsUpdate = true;
      lineGeo.attributes.color.needsUpdate = true;

      // Update data packet positions
      const pArr = packetGeo.attributes.position.array as Float32Array;
      packetRoutes.forEach((route, pIdx) => {
        route.progress += route.speed;
        if (route.progress >= 1) {
          route.progress = 0;
          route.from = Math.floor(Math.random() * nodeCount);
          route.to = Math.floor(Math.random() * nodeCount);
        }

        const pA = nodePositions[route.from];
        const pB = nodePositions[route.to];
        const currentP = pA.clone().lerp(pB, route.progress);

        pArr[pIdx * 3] = currentP.x;
        pArr[pIdx * 3 + 1] = currentP.y;
        pArr[pIdx * 3 + 2] = currentP.z;
      });
      packetGeo.attributes.position.needsUpdate = true;

      renderer.render(scene, camera);
    };

    animate();

    // Resize Handler
    const handleResize = () => {
      camera.aspect = window.innerWidth / window.innerHeight;
      camera.updateProjectionMatrix();
      renderer.setSize(window.innerWidth, window.innerHeight);
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

  return (
    <div
      ref={mountRef}
      className="fixed inset-0 w-full h-full pointer-events-none select-none z-0"
    />
  );
};
