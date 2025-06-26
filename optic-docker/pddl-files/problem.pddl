;; problem.pddl
(define (problem test-simple)
  (:domain logistics_simple)
  (:objects
    truck1            - truck
    locA locB         - place
    cityA cityB       - city
  )
  (:init
    ;; posizione iniziale
    (at truck1 locA)
    ;; appartenenza a città
    (in-city locA cityA)
    (in-city locB cityB)

    ;; link riflessivi e simmetrici fra città
    (link cityA cityA)
    (link cityB cityB)
    (link cityA cityB)
    (link cityB cityA)

    ;; tempi di viaggio e capacità
    (= (travel-time locA locB) 50)
    (= (max-distance truck1) 100)
    (= (distance-run truck1) 0)
  )
  (:goal
    (at truck1 locB)
  )
)
