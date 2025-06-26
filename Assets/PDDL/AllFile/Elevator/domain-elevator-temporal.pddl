;;; elevator-temporal domain

(define (domain elevator-temporal)
  (:requirements :typing :fluents :durative-actions)
  (:types
    floor person elevator
  )

  (:predicates
    ;; il passeggero p si trova al piano f
    (at-person   ?p - person ?f - floor)
    ;; p è dentro l’ascensore e
    (in          ?e - elevator ?p - person)
    ;; l’ascensore e si trova al piano f
    (at-elevator ?e - elevator ?f - floor)
    ;; f2 è sopra f1 (adiacenza)
    (above       ?f1 - floor ?f2 - floor)
    ;; il passeggero p ha obiettivo di target f
    (target      ?p - person ?f - floor)
    ;; p ha già raggiunto il target
    (reached     ?p - person)
  )

  (:functions
    ;; tempo per imbarcarsi/sbarcarsi di p
    (person-speed   ?p - person)
    ;; velocità di e
    (elevator-speed ?e - elevator)
    ;; distanza fra piani
    (floor-distance ?f1 - floor ?f2 - floor)
  )

  ;; --------------------------------------------------
  ;; Durative actions
  ;; --------------------------------------------------

  (:durative-action move-up
    :parameters (?e - elevator ?f1 ?f2 - floor)
    :duration (= ?duration
                  (/ (floor-distance ?f1 ?f2)
                     (elevator-speed ?e)))
    :condition (and
      (at start   (at-elevator ?e ?f1))
      (over all  (above       ?f1 ?f2))
    )
    :effect (and
      (at start (not (at-elevator ?e ?f1)))
      (at end   (at-elevator   ?e ?f2))
    )
  )

  (:durative-action move-down
    :parameters (?e - elevator ?f2 ?f1 - floor)
    :duration (= ?duration
                  (/ (floor-distance ?f1 ?f2)
                     (elevator-speed ?e)))
    :condition (and
      (at start   (at-elevator ?e ?f2))
      (over all  (above       ?f1 ?f2))
    )
    :effect (and
      (at start (not (at-elevator ?e ?f2)))
      (at end   (at-elevator   ?e ?f1))
    )
  )

  (:durative-action load
    :parameters (?p - person ?f - floor ?e - elevator)
    :duration (= ?duration (person-speed ?p))
    :condition (and
      (at start   (at-person   ?p ?f))
      (over all   (at-elevator ?e ?f))
    )
    :effect (and
      (at start (not (at-person ?p ?f)))
      (at end   (in           ?e ?p))
    )
  )

  (:durative-action unload
    :parameters (?p - person ?f - floor ?e - elevator)
    :duration (= ?duration (person-speed ?p))
    :condition (and
      (at start   (in           ?e ?p))
      (over all   (at-elevator  ?e ?f))
    )
    :effect (and
      (at start (not (in     ?e ?p)))
      (at end   (at-person   ?p ?f))
    )
  )

  ;; --------------------------------------------------
  ;; Instant action: segna raggiungimento target
  ;; --------------------------------------------------

  (:action reached
    :parameters (?p - person ?f - floor)
    :precondition (and
      (at-person ?p ?f)
      (target    ?p ?f)
      (not (reached ?p))
    )
    :effect (reached ?p)
  )
)
